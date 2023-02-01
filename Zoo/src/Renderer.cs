using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo;

public enum Depth
{
    Ground = -1,
    GroundCover = -2,
    Water = -3,
    Overlay = -4,
    YSorting = - 5,
    Debug = -8,
    UI = -9,
    Camera = -10
}

internal class Blit {
    public Texture2D Texture;
    public Rectangle SourceRect; 
    public Rectangle DestRect; 
    public Vector3   Origin; 
    public float     Rotation; 
    public float     PosZ;
    public Color     Tint;
    public Shader?   FragShader;
    public int?      PickId;
}

public class Renderer {
    // Constants
    public const int PixelScale = 2;
    
    // Resources
    public static  Shader OutlineShader      = Raylib.LoadShader(null, "assets/shaders/outline.fsh");
    private static Shader discardAlphaShader = Raylib.LoadShader(null, "assets/shaders/discard_alpha.fsh");
    private static Shader pickShader         = Raylib.LoadShader(null, "assets/shaders/pick.fsh");
    private static int    pickColourLoc      = Raylib.GetShaderLocation(pickShader, "pickColor");
    
    // Collections
    private List<Blit> blits = new();
    
    // State
    public  Camera          Camera = new();
    private RenderTexture2D pickBuffer;
    private Image           pickImage;
    
    public Renderer() {
        Debug.Log("Initialising Renderer");
        Raylib.SetTargetFPS(60);

        pickBuffer = Raylib.LoadRenderTexture(Game.ScreenWidth, Game.ScreenHeight);
        
        // Set outline colour
        int outlineColLoc = Raylib.GetShaderLocation(OutlineShader, "outlineCol");
        Raylib.SetShaderValue(OutlineShader, outlineColLoc, new Vector4(0.4f, 0.7f, 1f, 1f), ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        
        Rlgl.rlEnableDepthTest();
    }

    public void Update() {}

    public void Render() {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.DARKBLUE);
        
        Raylib.BeginMode3D(Camera.Cam);
        {
            Game.Render();
            
            Raylib.BeginShaderMode(discardAlphaShader);
            foreach (var blit in blits) {
                DoBlit(blit);
            }
            Raylib.EndShaderMode();
            
            
            Game.RenderLate();
        }
        Raylib.EndMode3D();
        
        RenderPickIdsToBuffer();
        pickImage = Raylib.LoadImageFromTexture(pickBuffer.texture);
        if (DebugSettings.DrawPickBuffer)
            RenderPickBuffer();
        
        Game.Render2D();
        Raylib.EndDrawing();
        
        blits.Clear();
    }

    public void OnScreenResized() {
        Raylib.UnloadRenderTexture(pickBuffer);
        pickBuffer = Raylib.LoadRenderTexture(Game.ScreenWidth, Game.ScreenHeight);
        
        Camera.OnScreenResized();
    }

    public float GetDepth(float yPos) {
        return Math.Clamp(yPos / World.LargerThanWorld, 0, 1) * -1 + (int)Depth.YSorting;
    }

    public void Blit(
        Texture2D  texture,
        Vector2    pos,
        float      depth  = 0,
        Vector2?   scale  = null,
        Vector2?   origin = null,
        Rectangle? source = null,
        Color?     color  = null,
        Shader?    fragShader = null,
        int?       pickId = null
    ) {
        scale  ??= new Vector2(texture.width, texture.height);
        origin ??= new Vector2(0, 0);
        source ??= new Rectangle(0, 0, 1, 1);
        color  ??= Color.WHITE;
        
        // Cull offscreen blits
        if (!IsPosOnScreen(pos, MathF.Max(scale.Value.X, scale.Value.Y))) return;
        
        var src = new Rectangle(
            source.Value.x      * texture.width,
            source.Value.y      * texture.height,
            source.Value.width  * texture.width,
            source.Value.height * texture.height
        );
        var scaledOrigin = origin.Value * scale.Value;
        
        blits.Add(new Blit {
            Texture = texture,
            SourceRect = src,
            DestRect = new Rectangle(pos.X, pos.Y, scale.Value.X, scale.Value.Y),
            Origin = new Vector3(scaledOrigin.X, scaledOrigin.Y, 0),
            Rotation = 0,
            PosZ = depth,
            Tint = color.Value,
            FragShader = fragShader,
            PickId = pickId
        });
    }

    private void DoBlit(Blit blit, bool picking = false) {
        // TODO: Look into whether switching shaders is expensive
        if (!picking && blit.FragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(blit.FragShader.Value);
        }
            
        Draw.DrawTexturePro3D(
            blit.Texture,
            blit.SourceRect,
            blit.DestRect,
            blit.Origin,
            blit.Rotation,
            blit.PosZ,
            blit.Tint
        );
        
        if (!picking && blit.FragShader.HasValue) {
            Raylib.EndShaderMode();
            Raylib.BeginShaderMode(discardAlphaShader);
        }
    }
    
    // Picking //

    private void RenderPickIdsToBuffer() {
        Raylib.BeginTextureMode(pickBuffer);
        Raylib.ClearBackground(Color.WHITE);
        Raylib.BeginMode3D(Camera.Cam);
        {
            foreach (var blit in blits) {
                if (!blit.PickId.HasValue) continue;
                
                Raylib.BeginShaderMode(pickShader);
                Raylib.SetShaderValue(pickShader, pickColourLoc, Colour.IntToColour(blit.PickId.Value).ToVector3(), ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                DoBlit(blit, true);
                Raylib.EndShaderMode();
            }
        }
        Raylib.EndMode3D();
        Raylib.EndTextureMode();
    }

    public void RenderPickBuffer() {
        Raylib.DrawTexturePro(
            pickBuffer.texture,
            new Rectangle(0, 0, pickBuffer.texture.width, -pickBuffer.texture.height),
            new Rectangle(0, Game.ScreenHeight - 112, 173, 112),
            new Vector2(0, 0),
            0,
            Color.WHITE
        );
    }
    
    public int GetPickIdAtPos(Vector2 screenPos) {
        var pixel = Raylib.GetImageColor(pickImage, screenPos.X.FloorToInt(), Game.ScreenHeight - screenPos.Y.FloorToInt());
        if (pixel.Equals(Color.WHITE)) return -1;
        return Colour.ColourToInt(pixel);
    }
    
    // Utility //
    
    public Vector2 ScreenToWorldPos(Vector2 screenPos) {
        var cameraCenter = (Camera.Position * Camera.Zoom) - new Vector2(Game.ScreenWidth/2f, Game.ScreenHeight/2f);
        return (screenPos + cameraCenter) / (World.WorldScale * Camera.Zoom);
    }
    
    // TODO: make sure this is correct since this is literally AI generated
    public Vector2 WorldToScreenPos(Vector2 worldPos) {
        return worldPos * (World.WorldScale * Camera.Zoom) - (Camera.Position * Camera.Zoom) + new Vector2(Game.ScreenWidth/2f, Game.ScreenHeight/2f);
    }

    public bool IsPosOnScreen(Vector2 pos, float margin = 0) {
        return pos.X > Camera.Position.X - Game.ScreenWidth/2  - margin && pos.X < Camera.Position.X + Game.ScreenWidth/2 + margin 
            && pos.Y > Camera.Position.Y - Game.ScreenHeight/2 - margin && pos.Y < Camera.Position.Y + Game.ScreenHeight/2 + margin;
    }

    public bool IsWorldPosOnScreen(Vector2 worldPos, float margin = 32) {
        var topLeft = ScreenToWorldPos(new Vector2(0, 0) - new Vector2(margin, margin));
        var bottomRight = ScreenToWorldPos(new Vector2(Game.ScreenWidth, Game.ScreenHeight) + new Vector2(margin, margin));
        
        return worldPos.X > topLeft.X && worldPos.X < bottomRight.X 
            && worldPos.Y > topLeft.Y && worldPos.Y < bottomRight.Y;
    }

    public bool IsWorldRectOnScreen(Rectangle rect, float margin = 32) {
        var topLeft     = ScreenToWorldPos(new Vector2(0, 0) - new Vector2(margin, margin));
        var bottomRight = ScreenToWorldPos(new Vector2(Game.ScreenWidth, Game.ScreenHeight) + new Vector2(margin, margin));

        return rect.x + rect.width > topLeft.X     &&
            rect.x                 < bottomRight.X &&
            rect.y + rect.height   > topLeft.Y     &&
            rect.y                 < bottomRight.Y;
    }
}