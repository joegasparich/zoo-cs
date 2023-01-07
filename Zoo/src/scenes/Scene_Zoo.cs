using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.util;

namespace Zoo; 

public class Scene_Zoo : Scene {
    // Constants
    private const string SceneName = "Zoo";
    
    // State
    public Zoo Zoo;

    public Scene_Zoo() : base(SceneName) {
        Zoo = new Zoo();
    }

    public override void Start() {
        Debug.Log("Creating new zoo");
        Zoo.Setup();
        
        var man = new Entity(new Vector2(0, 0));
        var renderer = man.AddComponent<RenderComponent>();
        renderer.Graphics.SetSprite(TEXTURES.KEEPER);
        man.AddComponent<AreaPathFollowComponent>();
        man.AddComponent<PhysicsComponent>();
        man.AddComponent<MoveComponent>();
        man.AddComponent<TestControllableComponent>();
        Game.RegisterEntity(man);
    }

    public override void Stop() {
        base.Stop();
        
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
    }
    
    public override void Serialise() {
        Find.SaveManager.ArchiveDeep("zoo", Zoo);
    }
}