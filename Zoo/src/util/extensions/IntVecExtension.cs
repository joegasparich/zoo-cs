using System.Numerics;

namespace Zoo.util; 

public static class IntVecExtension {
    public static float Distance(this IntVec2 tile, IntVec2 other) {
        return MathF.Sqrt(MathF.Pow(tile.X - other.X, 2) + MathF.Pow(tile.Y - other.Y, 2));
    }
    public static float DistanceSquared(this IntVec2 tile, IntVec2 other) {
        return MathF.Pow(tile.X - other.X, 2) + MathF.Pow(tile.Y - other.Y, 2);
    }
    public static bool InDistOf(this IntVec2 a, IntVec2 b, float range) {
        return a.DistanceSquared(b) < range * range;
    }
    public static Vector2 Centre(this IntVec2 tile) {
        return new Vector2(tile.X + 0.5f, tile.Y + 0.5f);
    }
}