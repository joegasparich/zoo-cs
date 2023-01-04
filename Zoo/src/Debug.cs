using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo; 

public static class Debug {
    public static void Log(string message) {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, message);
    }

    public static void Warn(string message) {
        Raylib.TraceLog(TraceLogLevel.LOG_WARNING, message);
    }
    
    public static void Error(string message) {
        Raylib.TraceLog(TraceLogLevel.LOG_ERROR, message);
    }
    
    public static void Assert(bool condition, string message = "") {
        if (!condition) {
            throw new Exception(message);
        }
    }
    
    public static void DrawLine(Vector2 start, Vector2 end, Color colour, bool worldScale) {
        var scale = worldScale ? World.WorldScale : 1;
        Draw.DrawLineV3D(start * scale, end * scale, colour, Depth.Debug.ToInt());
    }
    
    public static void DrawRect(Vector2 start, Vector2 dimensions, Color colour, bool worldScale) {
        var scale = worldScale ? World.WorldScale : 1;
        Draw.DrawRectangleV3D(start * scale, dimensions * scale, colour, Depth.Debug.ToInt());
    }
}