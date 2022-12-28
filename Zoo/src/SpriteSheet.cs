using Raylib_cs;

namespace Zoo; 

public class SpriteSheet {
    public int       CellWidth;
    public int       CellHeight;
    public string    TexturePath;
    public Texture2D Texture;

    public Rectangle GetCellBounds(int cellIndex) {
        var cols = Texture.width       / CellWidth;
        return GetCellBounds(cellIndex % cols, cellIndex / cols);
    }

    public Rectangle GetCellBounds(int col, int row) {
        var cols  = Texture.width  / CellWidth;
        var rows  = Texture.height / CellHeight;
        var xFrac = CellWidth      / (float)Texture.width;
        var yFrac = CellHeight     / (float)Texture.height;

        if (col >= cols || row >= rows) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, $"Spritesheet cell out of bounds ({col}, {row})");
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