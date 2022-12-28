using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Wall : Tool {
    private          WallData        currentWall;
    private          List<WallData>  allWalls;
    private          bool            isDragging;
    private          IntVec2         dragTile;
    private          Side            dragQuadrant;
    private readonly List<ToolGhost> ghosts = new();
    
    public override string   Name => "Wall Tool";
    public override ToolType Type => ToolType.Wall;

    public Tool_Wall(ToolManager tm) : base(tm) {
        allWalls = Find.Registry.GetAllWalls();
    }

    public override void Set() {
        Ghost.Type   = GhostType.SpriteSheet;
        Ghost.Snap   = true;
        Ghost.Scale  = new Vector2(1, 2);
        Ghost.Offset = new Vector2(0, -1);
        
        // Temp, should handle having no wall
        SetWall(Find.Registry.GetWall(WALLS.IRON_FENCE));
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging   = true;
            dragTile     = evt.mouseWorldPos.Floor();
            dragQuadrant = World.GetQuadrantAtPos(evt.mouseWorldPos);

            evt.Consume();
        }

        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = false;

            // Reverse so we are going from drag start to drag end
            ghosts.Reverse();

            while (ghosts.Any()) {
                var ghost = ghosts.Pop();
                if (ghost.CanPlace) {
                    Find.World.Walls.PlaceWallAtTile(currentWall, ghost.Pos.Floor(), dragQuadrant);
                }
            }

            evt.Consume();
        }
    }

    public override void Update() {
        var mousePos      = Find.Input.GetMouseWorldPos();
        var mouseQuadrant = World.GetQuadrantAtPos(mousePos);

        if (isDragging) {
            // Dragging
            Ghost.Visible = false;

            var xDif       = mousePos.Floor().X - dragTile.X;
            var yDif       = mousePos.Floor().Y - dragTile.Y;
            var horizontal = dragQuadrant is Side.North or Side.South;
            var length     = (horizontal ? MathF.Abs(xDif) : MathF.Abs(yDif)) + 1;

            // Push new ghosts to reach length
            while (ghosts.Count < length) {
                var ghost = new ToolGhost(toolManager) {
                    Type        = GhostType.SpriteSheet,
                    Follow      = false,
                    SpriteSheet = Ghost.SpriteSheet,
                    Offset      = Ghost.Offset,
                    Side        = dragQuadrant,
                    Scale       = Ghost.Scale
                };
                ghosts.Add(ghost);
            }

            var i = MathF.Floor(dragTile.X);
            var j = MathF.Floor(dragTile.Y);
            foreach(var ghost in ghosts) {
                ghost.Pos  = new Vector2(i, j);
                UpdateGhostSprite(ghost, dragQuadrant);

                if (horizontal) {
                    i += MathF.Sign(mousePos.Floor().X - i);
                } else {
                    j += MathF.Sign(mousePos.Floor().Y - j);
                }
            }

            // Pop additional ghosts
            while (ghosts.Count > length) {
                ghosts.Pop();
            }
        } else {
            Ghost.Visible = true;

            UpdateGhostSprite(Ghost, mouseQuadrant);
        }
    }

    public override void Render() {
        foreach (var ghost in ghosts) {
            ghost.Render();
        }
    }

    public override bool CanPlace(ToolGhost ghost) {
        var tile     = ghost.Pos.Floor();
        var quadrant = ghost.Side;
        
        var wall = Find.World.Walls.GetWallAtTile(tile, quadrant);

        if (wall == null) return false;
        if (wall.Exists) return false;
        
        var (v1, v2) = wall.GetVertices();
        if (Find.World.Elevation.GetElevationAtPos(v1) < 0) return false;
        if (Find.World.Elevation.GetElevationAtPos(v2) < 0) return false;
        
        var tiles = Find.World.Walls.GetAdjacentTiles(wall);
        Entity blockingObj = null;
        foreach(var t in tiles) {
            var obj = Find.World.GetTileObjectAtTile(t);
            if (obj != null && obj == blockingObj) return false;

            if (blockingObj == null && obj != null) blockingObj = obj;
        }

        return true;
    }

    private void UpdateGhostSprite(ToolGhost ghost, Side quadrant) {
        var wall = Find.World.Walls.GetWallAtTile(ghost.Pos.Floor(), quadrant);

        ghost.Visible = wall != null;
        if (!ghost.Visible) return;

        var (spriteIndex, elevation) = WallUtility.GetSpriteInfo(wall!);

        ghost.SpriteIndex = spriteIndex.ToInt();

        switch (quadrant) {
            case Side.North:
                ghost.Offset = new Vector2(0.0f, -2.0f - elevation);
                break;
            case Side.South:
                ghost.Offset = new Vector2(0.0f, -1.0f - elevation);
                break;
            case Side.West:
                ghost.Offset = new Vector2(-0.5f, -1.0f - elevation);
                break;
            case Side.East:
                ghost.Offset = new Vector2(0.5f, -1.0f - elevation);
                break;
        }
    }

    private void SetWall(WallData data) {
        currentWall       = data;
        Ghost.SpriteSheet = data.SpriteSheet;
    }
}