using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

// Most of these functions are just copied from here but with a zPos parameter added
// https://github.com/raysan5/raylib/blob/e5d332dea23e65f66e7e7b279dc712afeb9404c9/src/rshapes.c
public static class Draw {
    // Draw a line  (Vector version)
    public static void DrawLineV3D(Vector2 startPos, Vector2 endPos, Color color, float zPos)
    {
        Rlgl.rlCheckRenderBatchLimit(10);
        Rlgl.rlBegin(DrawMode.LINES);
        Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);
        Rlgl.rlVertex3f(startPos.X, startPos.Y, zPos);
        Rlgl.rlVertex3f(endPos.X, endPos.Y, zPos);
        Rlgl.rlEnd();
    }
    
    // Draw lines sequence
    public static void DrawLineStrip3D(Vector2[] points, Color color, float zPos)
    {
        if (points.Length >= 2)
        {
            Rlgl.rlCheckRenderBatchLimit(points.Length);
            Rlgl.rlBegin(DrawMode.LINES);
            Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

            for (int i = 0; i < points.Length - 1; i++)
            {
                Rlgl.rlVertex3f(points[i].X, points[i].Y, zPos);
                Rlgl.rlVertex3f(points[i + 1].X, points[i + 1].Y, zPos);
            }
            Rlgl.rlEnd();
        }
    }

    // Draw a color-filled rectangle with pro parameters
    public static void DrawRectanglePro3D(Rectangle rec, Vector2 origin, float rotation, Color color, float zPos)
    {
        Vector2 topLeft;
        Vector2 topRight;
        Vector2 bottomLeft;
        Vector2 bottomRight;

        // Only calculate rotation if needed
        if (rotation == 0.0f)
        {
            var x = rec.x - origin.X;
            var y = rec.y - origin.Y;
            topLeft = new Vector2( x, y );
            topRight = new Vector2( x + rec.width, y );
            bottomLeft = new Vector2( x, y + rec.height );
            bottomRight = new Vector2( x + rec.width, y + rec.height );
        }
        else
        {
            var sinRotation = MathF.Sin(JMath.DegToRad(rotation));
            var cosRotation = MathF.Cos(JMath.DegToRad(rotation));
            var x = rec.x;
            var y = rec.y;
            var dx = -origin.X;
            var dy = -origin.Y;

            topLeft.X = x + dx*cosRotation - dy*sinRotation;
            topLeft.Y = y + dx*sinRotation + dy*cosRotation;

            topRight.X = x + (dx + rec.width)*cosRotation - dy*sinRotation;
            topRight.Y = y + (dx + rec.width)*sinRotation + dy*cosRotation;

            bottomLeft.X = x + dx*cosRotation - (dy + rec.height)*sinRotation;
            bottomLeft.Y = y + dx*sinRotation + (dy + rec.height)*cosRotation;

            bottomRight.X = x + (dx + rec.width)*cosRotation - (dy + rec.height)*sinRotation;
            bottomRight.Y = y + (dx + rec.width)*sinRotation + (dy + rec.height)*cosRotation;
        }

        Rlgl.rlCheckRenderBatchLimit(10);
        Rlgl.rlBegin(DrawMode.TRIANGLES);

        Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

        Rlgl.rlVertex3f(topLeft.X, topLeft.Y, zPos);
        Rlgl.rlVertex3f(bottomLeft.X, bottomLeft.Y, zPos);
        Rlgl.rlVertex3f(topRight.X, topRight.Y, zPos);

        Rlgl.rlVertex3f(topRight.X, topRight.Y, zPos);
        Rlgl.rlVertex3f(bottomLeft.X, bottomLeft.Y, zPos);
        Rlgl.rlVertex3f(bottomRight.X, bottomRight.Y, zPos);

        Rlgl.rlEnd();
    }

    // Draw a color-filled rectangle (Vector version)
    // NOTE: On OpenGL 3.3 and ES2 we use QUADS to avoid drawing order issues
    public static void DrawRectangleV3D(Vector2 position, Vector2 size, Color color, float zPos) {
        DrawRectanglePro3D(new Rectangle(position.X, position.Y, size.X, size.Y), new Vector2(0.0f, 0.0f), 0.0f, color, zPos);
    }

    // Draw a triangle fan defined by points
    // NOTE: First vertex provided is the center, shared by all triangles
    // By default, following vertex should be provided in counter-clockwise order
    public static void DrawTriangleFan3D(Vector2[] points, Color color, float zPos)
    {
        if (points.Length >= 3)
        {
            Rlgl.rlCheckRenderBatchLimit((points.Length - 2)*4);
            
            Rlgl.rlBegin(DrawMode.QUADS);
            Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

            for (int i = 1; i < points.Length - 1; i++)
            {
                Rlgl.rlVertex3f(points[0].X, points[0].Y, zPos);
                Rlgl.rlVertex3f(points[i].X, points[i].Y, zPos);
                Rlgl.rlVertex3f(points[i + 1].X, points[i + 1].Y, zPos);
                Rlgl.rlVertex3f(points[i + 1].X, points[i + 1].Y, zPos);
            }
            Rlgl.rlEnd();
        }
    }
    
    public static void DrawTexturePro3D(
        Texture2D texture,
        Rectangle sourceRect,
        Rectangle destRect,
        Vector3 origin,
        float rotation,
        float posZ,
        Color tint
    ) {
        // Check if texture if valid
        if (texture.id <= 0) return;

        var flipX = false;
        
        if (sourceRect.width < 0) { flipX = true; sourceRect.width *= -1; }
        if (sourceRect.height < 0) sourceRect.y -= sourceRect.height;
        
        Rlgl.rlCheckRenderBatchLimit(4);
        
        Rlgl.rlSetTexture(texture.id);
        Rlgl.rlPushMatrix();
        Rlgl.rlTranslatef(destRect.x, destRect.y, 0);
        Rlgl.rlRotatef(rotation, 0.0f, 0.0f, 1.0f);
        Rlgl.rlTranslatef(-origin.X, -origin.Y, -origin.Z);
        
        Rlgl.rlBegin(DrawMode.QUADS);
        Rlgl.rlColor4ub(tint.r, tint.g, tint.b, tint.a);
        Rlgl.rlNormal3f(0.0f, 0.0f, 1.0f); // Normal vector pointing towards viewer
        
        // Bottom-left corner for texture and quad
        if (flipX) Rlgl.rlTexCoord2f((sourceRect.x + sourceRect.width) / texture.width, sourceRect.y / texture.height);
        else Rlgl.rlTexCoord2f(sourceRect.x / texture.width, sourceRect.y / texture.height);
        Rlgl.rlVertex3f(0.0f, 0.0f, posZ);

        // Bottom-right corner for texture and quad
        if (flipX) Rlgl.rlTexCoord2f((sourceRect.x + sourceRect.width) / texture.width, (sourceRect.y + sourceRect.height) / texture.height);
        else Rlgl.rlTexCoord2f(sourceRect.x / texture.width, (sourceRect.y + sourceRect.height) / texture.height);
        Rlgl.rlVertex3f(0.0f, destRect.height, posZ);

        // Top-right corner for texture and quad
        if (flipX) Rlgl.rlTexCoord2f(sourceRect.x / texture.width, (sourceRect.y + sourceRect.height) / texture.height);
        else Rlgl.rlTexCoord2f((sourceRect.x + sourceRect.width) / texture.width, (sourceRect.y + sourceRect.height) / texture.height);
        Rlgl.rlVertex3f(destRect.width, destRect.height, posZ);

        // Top-left corner for texture and quad
        if (flipX) Rlgl.rlTexCoord2f(sourceRect.x / texture.width, sourceRect.y / texture.height);
        else Rlgl.rlTexCoord2f((sourceRect.x + sourceRect.width) / texture.width, sourceRect.y / texture.height);
        Rlgl.rlVertex3f(destRect.width, 0.0f, posZ);
        
        Rlgl.rlEnd();
        Rlgl.rlPopMatrix();
        Rlgl.rlSetTexture(0);
    }
}