using System.Numerics;
using Zoo.util;

namespace Zoo.world; 

public static class WallUtility {
    public static Vector2 WallToWorldPosition(IntVec2 gridPos, Orientation orientation) {
        return orientation == Orientation.Horizontal 
            ? new Vector2(gridPos.X / 2.0f, gridPos.Y) 
            : new Vector2(gridPos.X / 2.0f, gridPos.Y + 0.5f);
    }

    public static (int, int, Orientation) GetGridPosition(IntVec2 tilePos, Side side) {
        return side switch {
            Side.North => (tilePos.X * 2 + 1, tilePos.Y, Orientation.Horizontal),
            Side.East => (tilePos.X * 2 + 2, tilePos.Y, Orientation.Vertical),
            Side.South => (tilePos.X * 2 + 1, tilePos.Y + 1, Orientation.Horizontal),
            Side.West => (tilePos.X * 2, tilePos.Y, Orientation.Vertical)
        };
    }

    public static (int, float) GetSpriteInfo(Wall wall, bool isDoor = false) {
        if (wall.Orientation == Orientation.Horizontal) {
            var left      = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.WorldPos.X - 0.5f, wall.WorldPos.Y));
            var right     = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.WorldPos.X + 0.5f, wall.WorldPos.Y));
            var elevation = MathF.Min(left, right);

            if (Math.Abs(left - right) < 0.001f) return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorHorizontal.ToInt() : WallSpriteIndex.Horizontal.ToInt(), elevation );
            if (left > right)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorHorizontal.ToInt() : WallSpriteIndex.HillWest.ToInt(), elevation );
            if (left < right)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorHorizontal.ToInt() : WallSpriteIndex.HillEast.ToInt(), elevation );
        } else {
            var up        = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.WorldPos.X, wall.WorldPos.Y - 0.5f));
            var down      = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.WorldPos.X, wall.WorldPos.Y + 0.5f));
            var elevation = MathF.Min(up, down);

            if (Math.Abs(up - down) < 0.001f) return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorVertical.ToInt() : WallSpriteIndex.Vertical.ToInt(), elevation );
            if (up > down)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorVertical.ToInt() : WallSpriteIndex.HillNorth.ToInt(), elevation );
            if (up < down)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorVertical.ToInt() : WallSpriteIndex.HillSouth.ToInt(), elevation + ElevationUtility.ElevationHeight );
        }

        return (WallSpriteIndex.Horizontal.ToInt(), 0.0f);
    }

    public static (IntVec2, IntVec2) GetVertices(this Wall wall) {
        return wall.Orientation switch {
            Orientation.Horizontal => (
                new Vector2(wall.WorldPos.X - 0.5f, wall.WorldPos.Y).Floor(), 
                new Vector2(wall.WorldPos.X + 0.5f, wall.WorldPos.Y).Floor()
            ),
            Orientation.Vertical => (
                new Vector2(wall.WorldPos.X, wall.WorldPos.Y - 0.5f).Floor(), 
                new Vector2(wall.WorldPos.X, wall.WorldPos.Y + 0.5f).Floor()
            )
        };
    }
}