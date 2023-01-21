using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo; 

public class Camera {
    // Constants
    private const float CameraSpeed    = 2f;
    private const float CameraZoomRate = 0.005f;
    private const float MinZoom        = 0.5f;
    private const float MaxZoom        = 10f;
    
    // State
    private Camera3D camera;
    private float    zoom = 1;
    private Vector2? dragStart;
    private Vector2? dragCameraOrigin;

    public Camera3D Cam      => camera;
    public Vector2  Position => camera.position.XY();
    public float    Zoom     => zoom;

    public Camera() {
        camera = new Camera3D {
            projection = CameraProjection.CAMERA_ORTHOGRAPHIC,
            fovy       = Game.ScreenHeight / zoom,
            position   = new Vector3(0, 0,  (int)Depth.Camera),
            up         = new Vector3(0, -1, 0)
        };

        dragStart        = Find.Input.GetMousePos();
        dragCameraOrigin = camera.target.XY();
    }

    public void OnInput(InputEvent evt) {
        // Keyboard Controls
        float zoomDelta = 0;
        if (evt.inputHeld == InputType.CameraLeft) {
            camera.position.X -= CameraSpeed / zoom;
            evt.Consume();
        }
        if (evt.inputHeld == InputType.CameraRight) {
            camera.position.X += CameraSpeed / zoom;
            evt.Consume();
        }
        if (evt.inputHeld == InputType.CameraUp) {
            camera.position.Y -= CameraSpeed / zoom;
            evt.Consume();
        }
        if (evt.inputHeld == InputType.CameraDown) {
            camera.position.Y += CameraSpeed / zoom;
            evt.Consume();
        }
        if (evt.inputHeld == InputType.CameraZoomIn) {
            zoomDelta += CameraZoomRate;
            evt.Consume();
        }
        if (evt.inputHeld == InputType.CameraZoomOut) {
            zoomDelta -= CameraZoomRate;
            evt.Consume();
        }
        
        // Mouse controls
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_MIDDLE) {
            dragStart        = evt.mousePos;
            dragCameraOrigin = camera.target.XY();
        }
        if (evt.mouseHeld == MouseButton.MOUSE_BUTTON_MIDDLE) {
            var newPos = dragCameraOrigin.Value + (dragStart.Value - evt.mousePos) / zoom;
            camera.position = new Vector3(newPos.X, newPos.Y, camera.position.Z);
        }

        if (evt.type == InputEventType.MouseScroll) {
            // TODO: Zoom towards mouse
            zoomDelta = evt.mouseScroll / 10.0f;
        }

        var normalisedZoomLog = JMath.Normalise(MathF.Log(zoom), MathF.Log(MinZoom), MathF.Log(MaxZoom));
        zoom = MathF.Exp(JMath.Lerp(MathF.Log(MinZoom), MathF.Log(MaxZoom), normalisedZoomLog + zoomDelta));
        zoom = JMath.Clamp(zoom, MinZoom, MaxZoom);
        camera.fovy = Game.ScreenHeight / zoom;
        camera.target = camera.position with { Z = 0 };
    }

    public void OnScreenResized() {
        camera.fovy = Game.ScreenHeight / zoom;
    }
}