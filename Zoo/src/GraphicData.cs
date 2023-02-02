using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Raylib_cs;
using Zoo.util;

namespace Zoo; 

public struct GraphicData {
    // Config
    public  Vector2   Origin = Vector2.Zero;
    public  Vector2   Scale  = Vector2.One;
    private Texture2D texture;

    [JsonProperty] private string spritePath = "";
    [JsonProperty] private int    cellWidth  = 0;
    [JsonProperty] private int    cellHeight = 0;

    // Properties
    public Texture2D Texture
    {
        get
        {
            if (texture.Empty())
                texture = Find.AssetManager.GetTexture(spritePath);

            return texture;
        }
        set => texture = value;
    }

    public int CellWidth  => cellWidth  == 0 ? Texture.width : cellWidth;
    public int CellHeight => cellHeight == 0 ? Texture.height : cellHeight;

    public GraphicData() {}

    public void SetSprite(string path)
    {
        spritePath = path;
        Texture = Find.AssetManager.GetTexture(spritePath);
    }

    public void Blit(Vector2 pos, float depth, Color colour, int scale = 1, int index = 0, int? pickId = null, Shader? fragShader = null) {
        var source = GetCellBounds(index);
        
        Find.Renderer.Blit(
            texture: Texture,
            pos: pos,
            depth: depth,
            scale: new Vector2(Texture.width * source.width, Texture.height * source.height) * scale,
            origin: Origin,
            source: source,
            color: colour,
            fragShader: fragShader,
            pickId: pickId
        );
    }
    
    public Rectangle GetCellBounds(int cellIndex) {
        if (Texture.Empty()) return new Rectangle(0, 0, 1, 1);
        
        var cols = Texture.width        / CellWidth;
        return GetCellBounds(cellIndex % cols, cellIndex / cols);
    }

    public Rectangle GetCellBounds(int col, int row) {
        if (Texture.Empty()) return new Rectangle(0, 0, 1, 1);
        
        var cols  = Texture.width  / CellWidth;
        var rows  = Texture.height / CellHeight;
        var xFrac = CellWidth     / (float)Texture.width;
        var yFrac = CellHeight    / (float)Texture.height;

        if (col >= cols || row >= rows) {
            Debug.Warn($"Spritesheet cell out of bounds ({col}, {row})");
            return new Rectangle();
        }

        return new Rectangle(
            col * xFrac,
            row * yFrac,
            xFrac,
            yFrac
        );
    }
}