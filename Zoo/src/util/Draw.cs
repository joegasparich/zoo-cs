using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

public static class Draw {
    // Draw a triangle fan defined by points
    // NOTE: First vertex provided is the center, shared by all triangles
    // By default, following vertex should be provided in counter-clockwise order
    public static void DrawTriangleFan3D(Vector2[] points, int pointCount, Color color, float zPos)
    {
        if (pointCount >= 3)
        {
            Rlgl.rlBegin(DrawMode.QUADS);
            Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

            for (int i = 1; i < pointCount - 1; i++)
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