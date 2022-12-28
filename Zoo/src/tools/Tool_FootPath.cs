using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_FootPath : Tool {
    private          FootPathData       currentFootPath;
    private          List<FootPathData> allFootPaths;
    private          bool               isDragging;
    private          IntVec2            dragTile;
    private readonly List<ToolGhost>    ghosts = new();

    public override string   Name => "Path Tool";
    public override ToolType Type => ToolType.FootPath;

    public Tool_FootPath(ToolManager tm) : base(tm) {
        allFootPaths = Find.Registry.GetAllFootPaths();
    }

    public override void Set() {
        Ghost.Type  = GhostType.SpriteSheet;
        Ghost.Snap  = true;
        Ghost.Scale = new Vector2(1, 2);
        Ghost.Offset = new Vector2(0, -1);
        
        // Temp, should handle having no path
        SetPath(Find.Registry.GetFootPath(FOOTPATHS.DIRT_PATH));
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = true;
            dragTile   = evt.mouseWorldPos.Floor();

            evt.Consume();
        }

        if (evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            isDragging = false;

            while (ghosts.Any()) {
                var ghost = ghosts.Pop();
                if (ghost.CanPlace) {
                    Find.World.FootPaths.PlacePathAtTile(currentFootPath, ghost.Pos.Floor());
                }
            }

            evt.Consume();
        }
    }

    public override void Update() {
        if (isDragging) {
            // Dragging
            Ghost.Visible = false;

            var xDif       = Find.Input.GetMouseWorldPos().X - dragTile.X;
            var yDif       = Find.Input.GetMouseWorldPos().Y - dragTile.Y;
            var horizontal = MathF.Abs(xDif) > MathF.Abs(yDif);
            var length     = (horizontal ? MathF.Abs(xDif) : MathF.Abs(yDif)) + 1;

            // Push new ghosts to reach length
            while (ghosts.Count < length) {
                var ghost = new ToolGhost(toolManager);
                ghost.Type        = GhostType.SpriteSheet;
                ghost.Follow      = false;
                ghost.SpriteSheet = Ghost.SpriteSheet;
                ghost.Offset      = Ghost.Offset;
                ghost.Scale       = Ghost.Scale;
                ghosts.Add(ghost);
            }

            var i = MathF.Floor(dragTile.X);
            var j = MathF.Floor(dragTile.Y);
            foreach(var ghost in ghosts) {
                ghost.Pos = new Vector2(i, j);
                UpdateGhostSprite(ghost);

                if (horizontal) {
                    i += Math.Sign(Find.Input.GetMouseWorldPos().Floor().X - i);
                } else {
                    j += Math.Sign(Find.Input.GetMouseWorldPos().Floor().Y - j);
                }
            }

            // Pop additional ghosts
            while (ghosts.Count > length) {
                ghosts.Pop();
            }
        } else {
            Ghost.Visible = true;

            UpdateGhostSprite(Ghost);
        }
    }

    public override void Render() {
        foreach (var ghost in ghosts) {
            ghost.Render();
        }
    }

    public override bool CanPlace(ToolGhost ghost) {
        var path = Find.World.FootPaths.GetPathAtTile(ghost.Pos.Floor());

        if (path == null) return false;
        if (path.Exists) return false;
        if (Find.World.Elevation.IsPositionSlopeCorner(path.Pos)) return false;
        if (Find.World.Elevation.IsTileWater(path.Pos)) return false;

        return true;
    }

    private void UpdateGhostSprite(ToolGhost ghost) {
        var path = Find.World.FootPaths.GetPathAtTile(ghost.Pos.Floor());

        ghost.Visible = path != null;
        if (!ghost.Visible || !ghost.CanPlace) return;

        var (spriteIndex, elevation) = FootPathUtility.GetSpriteInfo(path!);

        ghost.SpriteIndex = spriteIndex.ToInt();
        ghost.Offset      = new Vector2(0, -1 - elevation);
    }

    private void SetPath(FootPathData data) {
        currentFootPath = data;
        Ghost.SpriteSheet = data.SpriteSheet;
    }
}