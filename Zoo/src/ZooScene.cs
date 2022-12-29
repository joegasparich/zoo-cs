using System.Numerics;
using Raylib_cs;
using Zoo.entities;

namespace Zoo; 

public class ZooScene : Scene {
    public Zoo Zoo;
    
    // Test
    private Entity man;

    public ZooScene() : base("Zoo") {}

    public override void Start() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Creating new zoo");
        Zoo = new Zoo();
        Zoo.Setup();
        
        // Test
        man = new Entity(new Vector2(0, 0));
        var renderer   = man.AddComponent<RenderComponent>();
        renderer.SetSprite(TEXTURES.KEEPER);
        man.AddComponent<PathFollowComponent>();
        man.AddComponent<PhysicsComponent>();
        man.AddComponent<MoveComponent>();
        Game.RegisterEntity(man);
    }

    public override void Stop() {
        Zoo.Cleanup();
    }

    public override void PreUpdate() {
        Zoo.PreUpdate();
    }
    public override void Update() {
        Zoo.Update();
    }
    public override void PostUpdate() {
        Zoo.PostUpdate();
    }

    public override void Render() {
        Zoo.Render();
    }
    public override void RenderLate() {
        Zoo.RenderLate();
    }

    public override void OnGUI() {
        Zoo.OnGUI();
    }

    public override void OnInput(InputEvent evt) {
        Zoo.OnInput(evt);

        if (evt.consumed) return;

        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            man.GetComponent<PathFollowComponent>()!.PathTo(evt.mouseWorldPos);
        }
    }
}