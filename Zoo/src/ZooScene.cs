using System.Numerics;
using Raylib_cs;
using Zoo.entities;

namespace Zoo; 

public class ZooScene : Scene {
    public ZooScene() : base("Zoo") { }

    public override void Start() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Creating new zoo");
        
        // Test
        var testEntity = new Entity(new Vector2(0, 0));
        var renderer   = testEntity.AddComponent<RenderComponent>();
        renderer.SetSprite(Assets.Keeper);
        Game.RegisterEntity(testEntity);
    }
}