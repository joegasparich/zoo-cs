using System.Numerics;
using Raylib_cs;

namespace Zoo.util;

public static class JMath {
    // Modify values
    public static float Lerp(float from, float to,  float pct) =>  from + (to - from) * pct;
    public static float Normalise(float val,  float min, float max) =>  (val - min) / (max - min);
    public static float Clamp(float val,  float min, float max) =>  Math.Max(min, Math.Min(max, val));
    public static float Min(params float[] vals) => vals.Min();
    public static float Max(params float[] vals) => vals.Max();
    
    // Trig
    public static float DegToRad(float deg) => deg * (float)Math.PI / 180f;
    public static float RadToDeg(float rad) => rad * 180f / (float)Math.PI;
    
    // Vectors
    public static Vector2 XY(this Vector3 v) => new Vector2(v.X, v.Y);
    public static Vector2 XZ(this Vector3 v) => new Vector2(v.X, v.Z);
    public static Vector2 YZ(this Vector3 v) => new Vector2(v.Y, v.Z);
    
    // Collision
    public static bool PointInRect(Rectangle rect, Vector2 point) {
        return point.X >= rect.x && point.X <= rect.x + rect.width &&
               point.Y >= rect.y && point.Y <= rect.y + rect.height;        
    }
    
    public static bool PointInCircle(Vector2 circlePos, float radius, Vector2 point) {
        var dx = circlePos.X - point.X;
        var dy = circlePos.Y - point.Y;
        return dx * dx + dy * dy < radius * radius;
    }
    
    public static bool CircleIntersectsRect(Vector2 boxPos, Vector2 boxDim, Vector2 circlePos, float circleRad) {
        var distX = MathF.Abs(circlePos.X - boxPos.X - boxDim.X / 2);
        var distY = MathF.Abs(circlePos.Y - boxPos.Y - boxDim.Y / 2);

        if (distX > boxDim.X / 2 + circleRad) {
            return false;
        }
        if (distY > boxDim.Y / 2 + circleRad) {
            return false;
        }

        if (distX <= boxDim.X / 2) {
            return true;
        }
        if (distY <= boxDim.Y / 2) {
            return true;
        }

        var dx = distX - boxDim.X / 2;
        var dy = distY - boxDim.Y / 2;
        return dx * dx + dy * dy <= circleRad * circleRad;
    }
}