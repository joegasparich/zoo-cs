using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

public static class RaylibExtension {
    // Texture2D //
    public static bool Empty(this Texture2D tex) {
        return tex.id == 0;
    }
    public static Vector2 Dimensions(this Texture2D tex) {
        return new(tex.width, tex.height);
    }
    
    // Key //
    public static bool IsAlphanumeric(this KeyboardKey key) {
        return key is >= KeyboardKey.KEY_APOSTROPHE and <= KeyboardKey.KEY_GRAVE;
    }
}