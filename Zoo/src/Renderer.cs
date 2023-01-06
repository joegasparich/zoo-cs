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
    public int       PickId;
}

public class Renderer {
    // Constants
    public const  int   PixelScale     = 2;
    private const float CameraSpeed    = 2f;
    private const float CameraZoomRate = 0.005f;
    private const float MinZoom        = 0.5f;
    private const float MaxZoom        = 10f;
    
    // Resources
    private static Shader discardAlphaShader = Raylib.LoadShader(null, "assets/shaders/discard_alpha.fsh");
    private static Shader pickShader         = Raylib.LoadShader(null, "assets/shaders/pick.fsh");
    private static int    pickColourLoc      = Raylib.GetShaderLocation(pickShader, "pickColor");
    
    // Collections
    private List<Blit> blits = new();
    
    // State
    private Camera3D        camera;
    private float           zoom = 1;
    private Vector2?        dragStart;
    private Vector2?        dragCameraOrigin;
    private RenderTexture2D pickBuffer;
    
    public Renderer() {
        Debug.Log("Initialising Renderer");
        Raylib.SetTargetFPS(60);

        camera.projection = CameraProjection.CAMERA_ORTHOGRAPHIC;
        camera.fovy       = Game.ScreenHeight / zoom;
        camera.position   = new Vector3(0, 0, (int)Depth.Camera);
        camera.up         = new Vector3(0, -1, 0);
        
        dragStart        = Find.Input.GetMousePos();
        dragCameraOrigin = camera.target.XY();

        pickBuffer = Raylib.LoadRenderTexture(Game.ScreenWidth, Game.ScreenHeight);
        
        Rlgl.rlEnableDepthTest();
    }

    public void Update() {
        // Camera movement
        // TODO: Refactor this out somewhere and use input manager

        float inputHorizontal = Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT) - Raylib.IsKeyDown(KeyboardKey.KEY_LEFT);
        float inputVertical = Raylib.IsKeyDown(KeyboardKey.KEY_DOWN) - Raylib.IsKeyDown(KeyboardKey.KEY_UP);

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_MIDDLE)) {
            dragStart        = Find.Input.GetMousePos();
            dragCameraOrigin = camera.target.XY();
        }
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE)) {
            var newPos = dragCameraOrigin.Value + (dragStart.Value - Find.Input.GetMousePos()) / zoom;
            camera.position = new Vector3(newPos.X, newPos.Y, camera.position.Z);
        }
        
        camera.position.X += inputHorizontal * CameraSpeed / zoom;
        camera.position.Y += inputVertical * CameraSpeed / zoom;
        camera.target     =  camera.position with { Z = 0 };
        
        // Camera zoom
        float zoomDelta = Raylib.GetMouseWheelMove() / 10.0f;

        if (Raylib.IsKeyDown(KeyboardKey.KEY_COMMA)) zoomDelta  += CameraZoomRate;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_PERIOD)) zoomDelta -= CameraZoomRate;

        // TODO: Zoom towards mouse

        // Do zoom
        var normalisedZoomLog = JMath.Normalise(MathF.Log(zoom), MathF.Log(MinZoom), MathF.Log(MaxZoom));
        zoom = MathF.Exp(JMath.Lerp(MathF.Log(MinZoom), MathF.Log(MaxZoom), normalisedZoomLog + zoomDelta));
        zoom = JMath.Clamp(zoom, MinZoom, MaxZoom);
        
        camera.fovy = Game.ScreenHeight / zoom;
    }

    public void Render() {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.DARKBLUE);
        
        Raylib.BeginMode3D(camera);
        {
            Game.Render3D();
            
            Raylib.BeginShaderMode(discardAlphaShader);
            foreach (var blit in blits) {
                DoBlit(blit);
            }

            Raylib.EndShaderMode();
        }
        Raylib.EndMode3D();
        
        RenderPickIdsToBuffer();
        // TODO: Toggleable
        RenderPickBuffer();
        
        Game.Render2D();
        Raylib.EndDrawing();
        
        blits.Clear();
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
        int        pickId = 0
    ) {
        // Cull offscreen blits
        // TODO: Calculate margin
        if (!IsPosOnScreen(pos, 100)) return;
        
        scale  ??= new Vector2(1, 1);
        origin ??= new Vector2(0, 0);
        source ??= new Rectangle(0, 0, 1, 1);
        color  ??= Color.WHITE;
        
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
            PickId = pickId
        });
    }

    private void DoBlit(Blit blit) {
        Draw.DrawTexturePro3D(
            blit.Texture,
            blit.SourceRect,
            blit.DestRect,
            blit.Origin,
            blit.Rotation,
            blit.PosZ,
            blit.Tint
        );
    }
    
    // Picking //

    private void RenderPickIdsToBuffer() {
        Raylib.BeginTextureMode(pickBuffer);
        Raylib.ClearBackground(Color.WHITE);
        Raylib.BeginMode3D(camera);
        {
            foreach (var blit in blits) {
                Raylib.BeginShaderMode(pickShader);
                Raylib.SetShaderValue(pickShader, pickColourLoc, Colour.IntToColour(blit.PickId).ToVector3(), ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                DoBlit(blit);
                Raylib.EndShaderMode();
            }
        }
        Raylib.EndMode3D();
        Raylib.EndTextureMode();
    }

    public void RenderPickBuffer() {
        Raylib.DrawTexturePro(
            pickBuffer.texture,
            new Rectangle(0, 0,                       pickBuffer.texture.width, -pickBuffer.texture.height),
            new Rectangle(0, Game.ScreenHeight - 112, 173,                      112),
            new Vector2(0, 0),
            0,
            Color.WHITE
        );
    }
    
    public int GetPickIdAtPos(Vector2 screenPos) {
        var image = Raylib.LoadImageFromTexture(pickBuffer.texture);
        var pixel = Raylib.GetImageColor(image, screenPos.X.FloorToInt(), Game.ScreenHeight - screenPos.Y.FloorToInt());
        return Colour.ColourToInt(pixel);
    }
    
    // Utility //
    
    public Vector2 ScreenToWorldPos(Vector2 screenPos) {
        var cameraCenter = (camera.position * zoom).XY() - new Vector2(Game.ScreenWidth/2f, Game.ScreenHeight/2f);
        return (screenPos + cameraCenter) / (World.WorldScale * zoom);
    }
    
    // TODO: make sure this is correct since this is literally AI generated
    public Vector2 WorldToScreenPos(Vector2 worldPos) {
        return worldPos * (World.WorldScale * zoom) - (camera.position * zoom).XY() + new Vector2(Game.ScreenWidth/2f, Game.ScreenHeight/2f);
    }

    public bool IsPosOnScreen(Vector2 pos, float margin = 0) {
        return pos.X > camera.position.X - Game.ScreenWidth/2  - margin && pos.X < camera.position.X + Game.ScreenWidth/2 + margin 
            && pos.Y > camera.position.Y - Game.ScreenHeight/2 - margin && pos.Y < camera.position.Y + Game.ScreenHeight/2 + margin;
    }

    // TODO: move these to a util class
    public bool IsWorldPosOnScreen(Vector2 worldPos, float margin = 32) {
        var topLeft = ScreenToWorldPos(new Vector2(0, 0) - new Vector2(margin, margin));
        var bottomRight = ScreenToWorldPos(new Vector2(Game.ScreenWidth, Game.ScreenHeight) + new Vector2(margin, margin));
        
        return worldPos.X > topLeft.X && worldPos.X < bottomRight.X 
            && worldPos.Y > topLeft.Y && worldPos.Y < bottomRight.Y;
    }

    public bool IsRectangleOnScreen(Rectangle rect) {
        var topLeft     = ScreenToWorldPos(new Vector2(0, 0));
        var bottomRight = ScreenToWorldPos(new Vector2(Game.ScreenWidth, Game.ScreenHeight));

        return rect.x + rect.width > topLeft.X     &&
            rect.x                 < bottomRight.X &&
            rect.y + rect.height   > topLeft.Y     &&
            rect.y                 < bottomRight.Y;
    }
}