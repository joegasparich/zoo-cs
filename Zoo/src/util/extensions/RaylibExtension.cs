using Raylib_cs;

namespace Zoo.util; 

public static class RaylibExtension {
    // Texture2D //
    public static bool Empty(this Texture2D tex) {
        return tex.id == 0;
    }
    
    // Key //
    public static bool IsAlphanumeric(this KeyboardKey key) {
        return key is >= KeyboardKey.KEY_APOSTROPHE and <= KeyboardKey.KEY_GRAVE;
    }
}