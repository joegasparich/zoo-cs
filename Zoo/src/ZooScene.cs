using System.Numerics;
using Raylib_cs;
using Zoo.entities;

namespace Zoo; 

public class ZooScene : Scene {
    public Zoo Zoo;

    public ZooScene() : base("Zoo") {}

    public override void Start() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Creating new zoo");
        Zoo = new Zoo();
        Zoo.Setup();
        
        // Test
        var testEntity = new Entity(new Vector2(0, 0));
        var renderer   = testEntity.AddComponent<RenderComponent>();
        renderer.SetSprite(TEXTURES.KEEPER);
        Game.RegisterEntity(testEntity);
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
    }
}