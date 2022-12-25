using System.Numerics;
using Raylib_cs;

namespace Zoo.util; 

public static class Draw {
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