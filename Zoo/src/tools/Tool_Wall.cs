using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Wall : Tool {
    // Constants
    private const int ButtonSize = 30;
    
    // References
    private List<WallDef> allWalls;
    
    // Virtual Properties
    public override string   Name => "Wall Tool";
    public override ToolType Type => ToolType.Wall;
    
    // State
    private          WallDef?       currentWall;
    private          bool            isDragging;
    private          IntVec2         dragTile;
    private          Side            dragQuadrant;
    private readonly List<ToolGhost> ghosts = new();

    public Tool_Wall(ToolManager tm) : base(tm) {
        allWalls = Find.AssetManager.GetAllDefs<WallDef>();
    }

    public override void Set() {
        Ghost.Type   = GhostType.Sprite;
        Ghost.Snap   = true;
        Ghost.Scale  = new Vector2(1, 2);
        Ghost.Offset = new Vector2(0, -1);
    }

    public override void OnInput(InputEvent evt) {
        if (currentWall == null) return;
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging   = true;
            dragTile     = evt.mouseWorldPos.Floor();
            dragQuadrant = World.GetQuadrantAtPos(evt.mouseWorldPos);

            evt.Consume();
        }

        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = false;

            // Reverse so we are going from drag start to drag end
            // This is to make sure we don't constantly check for loops (From memory?)
            ghosts.Reverse();
            
            List<(IntVec2, Side)> undoData = new();

            while (ghosts.Any()) {
                var ghost = ghosts.Pop();
                if (ghost.CanPlace) {
                    Find.World.Walls.PlaceWallAtTile(currentWall, ghost.Pos.Floor(), dragQuadrant);
                    undoData.Add((ghost.Pos.Floor(), dragQuadrant));
                }
            }
            
            toolManager.PushAction(new ToolAction() {
                Name = "Place walls",
                Data = undoData,
                Undo = (data) => {
                    foreach (var (tile, side) in (List<(IntVec2, Side)>) data) {
                        Find.World.Walls.RemoveWallAtTile(tile, side);
                    }
                }
            });

            evt.Consume();
        }
    }

    public override void Update() {
        if (currentWall == null) return;
        
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
                    Type     = GhostType.Sprite,
                    Follow   = false,
                    Graphics = Ghost.Graphics,
                    Offset   = Ghost.Offset,
                    Side     = dragQuadrant,
                    Scale    = Ghost.Scale
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
        if (currentWall == null) return;
        
        foreach (var ghost in ghosts) {
            ghost.Render();
        }
    }
    
    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immWallPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var wall in allWalls) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);

                if (GUI.ButtonEmpty(buttonRect, selected: currentWall != null && currentWall.Id == wall.Id))
                    SetWall(wall);
                
                GUI.DrawSubTexture(buttonRect, wall.GraphicData.Sprite, wall.GraphicData.GetCellBounds(0).BottomPct(0.25f));
                
                i++;
            }
        });
    }

    public override bool CanPlace(ToolGhost ghost) {
        if (currentWall == null) return false;
        
        var tile     = ghost.Pos.Floor();
        var quadrant = ghost.Side;
        
        var wall = Find.World.Walls.GetWallAtTile(tile, quadrant);

        if (wall == null) return false;
        if (wall.Exists) return false;
        
        var (v1, v2) = wall.GetVertices();
        if (Find.World.Elevation.GetElevationAtPos(v1) < 0) return false;
        if (Find.World.Elevation.GetElevationAtPos(v2) < 0) return false;
        
        var    tiles       = wall.GetAdjacentTiles();
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

        ghost.SpriteIndex = (int)spriteIndex;

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

    private void SetWall(WallDef data) {
        currentWall = data;
            
        if (currentWall != null) {
            Ghost.Graphics = data.GraphicData.DeepCopy();
            Ghost.Visible  = true;
        } else {
            Ghost.Visible = false;
        }
    }
}