using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;
using Zoo.util;

namespace Zoo; 

public struct GraphicData {
    // Config
    public                 string  SpritePath = "";
    public                 Vector2 Origin     = Vector2.Zero;
    public                 Vector2 Scale      = Vector2.One;
    [JsonProperty] private int     cellWidth  = 0;
    [JsonProperty] private int     cellHeight = 0;

    // Properties
    public Texture2D Sprite     => Find.AssetManager.GetTexture(SpritePath);
    public int       CellWidth  => cellWidth  == 0 ? Sprite.width : cellWidth;
    public int       CellHeight => cellHeight == 0 ? Sprite.height : cellHeight;

    public GraphicData() {}

    public void Blit(Vector2 pos, float depth, Color colour, int index = 0, int pickId = 0) {
        var source = GetCellBounds(index);
        
        Find.Renderer.Blit(
            texture: Sprite,
            pos: pos,
            depth: depth,
            scale: new Vector2(Sprite.width * source.width, Sprite.height * source.height) * Renderer.PixelScale,
            origin: Origin,
            source: source,
            color: colour,
            pickId: pickId
        );
    }
    
    public Rectangle GetCellBounds(int cellIndex) {
        if (Sprite.Empty()) return new Rectangle(0, 0, 1, 1);
        
        var cols = Sprite.width        / CellWidth;
        return GetCellBounds(cellIndex % cols, cellIndex / cols);
    }

    public Rectangle GetCellBounds(int col, int row) {
        if (Sprite.Empty()) return new Rectangle(0, 0, 1, 1);
        
        var cols  = Sprite.width  / CellWidth;
        var rows  = Sprite.height / CellHeight;
        var xFrac = CellWidth     / (float)Sprite.width;
        var yFrac = CellHeight    / (float)Sprite.height;

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