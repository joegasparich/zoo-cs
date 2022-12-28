using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_Door : Tool {
    public override string   Name => "Door Tool";
    public override ToolType Type => ToolType.Door;

    public Tool_Door(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type   = GhostType.SpriteSheet;
        Ghost.Snap   = true;
        Ghost.Scale  = new Vector2(1, 2);
        Ghost.Offset = new Vector2(0, -1);
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            var mouseQuadrant = World.GetQuadrantAtPos(evt.mouseWorldPos);

            if (Ghost.CanPlace) {
                var wall = Find.World.Walls.GetWallAtTile(evt.mouseWorldPos.Floor(), mouseQuadrant);
                Find.World.Walls.PlaceDoor(wall!);
            }
            
            evt.Consume();
        }
    }

    public override void Update() {
        var wall = Find.World.Walls.GetWallAtTile(Ghost.Pos.Floor(), Ghost.Side);
        
        Ghost.Visible = wall is { Exists: true };

        if (!Ghost.Visible) return;

        var (spriteIndex, elevation) = WallUtility.GetSpriteInfo(wall, true);
        Ghost.SpriteSheet = wall.Data!.SpriteSheet;
        Ghost.SpriteIndex = spriteIndex.ToInt();

        Ghost.Offset = Ghost.Side switch {
            Side.North => new Vector2(0.0f, -2.0f - elevation),
            Side.South => new Vector2(0.0f, -1.0f - elevation),
            Side.West => new Vector2(-0.5f, -1.0f - elevation),
            Side.East => new Vector2(0.5f, -1.0f - elevation)
        };
    }

    public override bool CanPlace(ToolGhost ghost) {
        var tile     = ghost.Pos.Floor();
        var quadrant = ghost.Side;
        
        var wall = Find.World.Walls.GetWallAtTile(tile, quadrant);

        if (wall is not { Exists: true }) return false;
        if (Find.World.Walls.IsWallSloped(wall)) return false;

        return true;
    }
}