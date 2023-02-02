using Raylib_cs;

namespace Zoo.util;

public class CachedTexture {
    private string path;

    public Texture2D Texture => Find.AssetManager.GetTexture(path);

    public CachedTexture(string path) {
        this.path = path;
    }
}