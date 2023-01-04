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

public class Renderer {
    // Constants
    public const  int   PixelScale     = 2;
    private const float CameraSpeed    = 2f;
    private const float CameraZoomRate = 0.005f;
    private const float MinZoom        = 0.5f;
    private const float MaxZoom        = 10f;
    
    private Camera3D camera;
    private Shader   discardAlphaShader;
    private float    zoom = 1;

    private Vector2? dragStart;
    private Vector2? dragCameraOrigin;
    
    public Renderer() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Initialising Renderer");
        Raylib.SetTargetFPS(60);

        discardAlphaShader = Raylib.LoadShader(null, "assets/shaders/discard_alpha.fs");

        camera.projection = CameraProjection.CAMERA_ORTHOGRAPHIC;
        camera.fovy       = Game.ScreenHeight / zoom;
        camera.position   = new Vector3(0, 0, Depth.Camera.ToInt());
        camera.up         = new Vector3(0, -1, 0);
        
        dragStart        = Find.Input.GetMousePos();
        dragCameraOrigin = camera.target.XY();
        
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

    public void BeginDrawing() {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.DARKBLUE);
        
        Raylib.BeginShaderMode(discardAlphaShader);
    }

    public void EndDrawing() {
        Raylib.EndShaderMode();
        Raylib.EndDrawing();
    }
    
    public void Begin3D() {
        Raylib.BeginMode3D(camera);
    }
    
    public void End3D() {
        Raylib.EndMode3D();
    }

    public float GetDepth(float yPos) {
        return Math.Clamp(yPos / World.LargerThanWorld, 0, 1) * -1 + Depth.YSorting.ToInt();
    }

    public void Blit(
        Texture2D texture,
        Vector2    pos,
        float      depth  = 0,
        Vector2?   scale  = null,
        Vector2?   origin = null,
        Rectangle? source = null,
        Color?     color  = null
    ) {
        scale  ??= new Vector2(1, 1);
        origin ??= new Vector2(0, 0);
        source ??= new Rectangle(0, 0, 1, 1);
        color  ??= Color.WHITE;
        
        var src = new Rectangle(
            source.Value.x * texture.width,
            source.Value.y * texture.height,
            source.Value.width * texture.width,
            source.Value.height * texture.height
        );
        var scaledOrigin = origin.Value * scale.Value;
        
        Draw.DrawTexturePro3D(
            texture,
            src,
            new Rectangle(pos.X, pos.Y, scale.Value.X, scale.Value.Y),
            new Vector3(scaledOrigin.X, scaledOrigin.Y, 0),
            0,
            depth,
            color.Value
        );
    }
    
    public Vector2 ScreenToWorldPos(Vector2 screenPos) {
        var cameraCenter = (camera.position * zoom).XY() - new Vector2(Game.ScreenWidth/2f, Game.ScreenHeight/2f);
        return (screenPos + cameraCenter) / (World.WorldScale * zoom);
    }
    
    // TODO: make sure this is correct since this is literally AI generated
    public Vector2 WorldToScreenPos(Vector2 worldPos) {
        return worldPos * (World.WorldScale * zoom) - (camera.position * zoom).XY() + new Vector2(Game.ScreenWidth/2f, Game.ScreenHeight/2f);
    }

    public bool IsPositionOnScreen(Vector2 worldPos, float margin) {
        var topLeft = ScreenToWorldPos(new Vector2(0, 0));
        var bottomRight = ScreenToWorldPos(new Vector2(Game.ScreenWidth, Game.ScreenHeight));
        
        return worldPos.X > topLeft.X && worldPos.X < bottomRight.X 
            && worldPos.Y > topLeft.Y && worldPos.Y < bottomRight.Y;
    }

    public bool IsRectangleOnScreen(Rectangle rect) {
        var topLeft     = ScreenToWorldPos(new Vector2(0,                0));
        var bottomRight = ScreenToWorldPos(new Vector2(Game.ScreenWidth, Game.ScreenHeight));

        return rect.x + rect.width > topLeft.X     &&
            rect.x                 < bottomRight.X &&
            rect.y + rect.height   > topLeft.Y     &&
            rect.y                 < bottomRight.Y;
    }
}