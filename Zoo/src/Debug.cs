﻿using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo; 

public static class Debug {
    public static void Log(string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null) {
        var fileName = file.Substring(file.LastIndexOf('\\') + 1);
            
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("{0,-10}{1,-150}{2} {3}", "[info]", message, fileName, $"{caller}:{lineNumber}");
    }

    public static void Warn(string message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null) {
        var fileName = file.Substring(file.LastIndexOf('\\') + 1);
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("{0,-10}{1,-150}{2} {3}", "[warn]", message, fileName, $"{caller}:{lineNumber}");
    }
    
    public static void Error(string message, Exception? e = null, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null) {
        var fileName = file.Substring(file.LastIndexOf('\\') + 1);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("{0,-10}{1,-150}{2} {3}", "[error]", message, fileName, $"{caller}:{lineNumber}");
        if (e != null) Console.WriteLine(e);
    }

    public static void Assert(bool condition, string message = "") {
        if (!condition) {
            throw new Exception(message);
        }
    }
    
    public static void DrawLine(Vector2 start, Vector2 end, Color colour, bool worldScale) {
        var scale = worldScale ? World.WorldScale : 1;
        Draw.DrawLineV3D(start * scale, end * scale, colour, (int)Depth.Debug);
    }
    
    public static void DrawRect(Vector2 start, Vector2 dimensions, Color colour, bool worldScale) {
        var scale = worldScale ? World.WorldScale : 1;
        Draw.DrawRectangleV3D(start * scale, dimensions * scale, colour, (int)Depth.Debug);
    }
}
