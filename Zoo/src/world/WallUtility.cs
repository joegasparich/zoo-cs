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

    public static (WallSpriteIndex, float) GetSpriteInfo(Wall wall, bool isDoor = false) {
        if (wall.Orientation == Orientation.Horizontal) {
            var left      = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.Pos.X - 0.5f, wall.Pos.Y));
            var right     = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.Pos.X + 0.5f, wall.Pos.Y));
            var elevation = MathF.Min(left, right);

            if (left.NearlyEquals(right)) return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorHorizontal : WallSpriteIndex.Horizontal, elevation );
            if (left > right)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorHorizontal : WallSpriteIndex.HillWest, elevation );
            if (left < right)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorHorizontal : WallSpriteIndex.HillEast, elevation );
        } else {
            var up        = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.Pos.X, wall.Pos.Y - 0.5f));
            var down      = Find.World.Elevation.GetElevationAtPos(new Vector2(wall.Pos.X, wall.Pos.Y + 0.5f));
            var elevation = MathF.Min(up, down);

            if (up.NearlyEquals(down)) return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorVertical : WallSpriteIndex.Vertical, elevation );
            if (up > down)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorVertical : WallSpriteIndex.HillNorth, elevation );
            if (up < down)                    return ( isDoor || wall.IsDoor ? WallSpriteIndex.DoorVertical : WallSpriteIndex.HillSouth, elevation + ElevationUtility.ElevationHeight );
        }

        return (WallSpriteIndex.Horizontal, 0.0f);
    }

    public static (IntVec2, IntVec2) GetVertices(this Wall wall) {
        return wall.Orientation switch {
            Orientation.Horizontal => (
                new Vector2(wall.Pos.X - 0.5f, wall.Pos.Y).Floor(), 
                new Vector2(wall.Pos.X + 0.5f, wall.Pos.Y).Floor()
            ),
            Orientation.Vertical => (
                new Vector2(wall.Pos.X, wall.Pos.Y - 0.5f).Floor(), 
                new Vector2(wall.Pos.X, wall.Pos.Y + 0.5f).Floor()
            )
        };
    }

    // Doesn't include blueprint walls currently
    private static List<Wall> adjacentWalls = new ();
    public static Wall[] GetAdjacentWalls(this Wall wall) {
        adjacentWalls.Clear();
        var x = wall.GridPos.X;
        var y = wall.GridPos.Y;

        if (wall.Orientation == Orientation.Horizontal) {
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x - 2, y))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x - 2, y))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x + 2, y))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x + 2, y))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y - 1)) is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y - 1))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y - 1)) is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y - 1))!);
        } else {
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y + 1)) is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x - 1, y + 1))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y + 1)) is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x + 1, y + 1))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x, y + 1))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x, y + 1))!);
            if (Find.World.Walls.GetWallByGridPos(new IntVec2(x, y - 1))     is { Exists: true }) adjacentWalls.Add(Find.World.Walls.GetWallByGridPos(new IntVec2(x, y - 1))!);
        }

        return adjacentWalls.ToArray();
    }
}