using Zoo.world;

namespace Zoo.util; 

public static class IntVecExtension {
    public static float Distance(this IntVec2 tile, IntVec2 other) {
        return MathF.Sqrt(MathF.Pow(tile.X - other.X, 2) + MathF.Pow(tile.Y - other.Y, 2));
    }
    public static float DistanceSquared(this IntVec2 tile, IntVec2 other) {
        return MathF.Pow(tile.X - other.X, 2) + MathF.Pow(tile.Y - other.Y, 2);
    }

    public static bool IsWalkable(this IntVec2 tile, AccessibilityType accessibilityType) {
        return Find.World.IsTileWalkable(tile, accessibilityType);
    }
}