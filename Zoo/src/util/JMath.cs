using System.Numerics;

namespace Zoo.util; 

public static class JMath {
    public static int RoundToInt(float f) => (int) Math.Round(f);
    
    public static float Lerp(float from, float to, float pct) =>  from + (to - from) * pct;
    public static float Normalise(float val, float min, float max) =>  (val - min) / (max - min);
    public static float Clamp(float val, float min, float max) =>  Math.Max(min, Math.Min(max, val));
    
    // Vectors
    public static Vector2 XY(this Vector3 v) => new Vector2(v.X, v.Y);
    public static Vector2 XZ(this Vector3 v) => new Vector2(v.X, v.Z);
    public static Vector2 YZ(this Vector3 v) => new Vector2(v.Y, v.Z);
}