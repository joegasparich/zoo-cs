using System.Numerics;
using System.Text.Json.Serialization;
using Raylib_cs;
using Zoo.util;

namespace Zoo; 

// This class is per objectType so can't be changed on a per object basis
// Any per object changes need to be sent through the blit params
public class GraphicData {
    // Config
    public  readonly string    SpritePath;
    public  readonly Vector2   Origin = Vector2.Zero;
    public  readonly Vector2   Scale  = Vector2.One;
    private readonly int       cellWidth;
    private readonly int       cellHeight;

    // Properties
    public Texture2D Sprite     => Find.AssetManager.GetTexture(SpritePath);
    public int       CellWidth  => cellWidth  == 0 ? Sprite.width : cellWidth;
    public int       CellHeight => cellHeight == 0 ? Sprite.height : cellHeight;

    [JsonConstructor]
    public GraphicData(string spritePath, int cellWidth, int cellHeight) {
        SpritePath = spritePath;
        
        this.cellWidth  = cellWidth;
        this.cellHeight = cellHeight;
    }

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