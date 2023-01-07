using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.entities; 

public class TestControllableComponent : Component {
    // References
    private PathFollowComponent pathfinder;

    protected override Type[] Dependencies => new[] { typeof(PathFollowComponent) };

    public TestControllableComponent(Entity entity) : base(entity) {}

    public override void Start() {
        base.Start();
        
        pathfinder = entity.GetComponent<PathFollowComponent>();
        Debug.Assert(pathfinder != null);
    }

    public override void Render() {
        var path = pathfinder.GetPath();
        if (!path.NullOrEmpty()) {
            for (var i = 1; i < path.Count; i++) {
                Debug.DrawLine(
                    path[i -1] + new Vector2(0.5f, 0.5f),
                    path[i] + new Vector2(0.5f, 0.5f),
                    Color.RED, 
                    true
                );
            }
        }
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            if (Find.World.IsPositionInMap(evt.mouseWorldPos)) {
                pathfinder.PathTo(evt.mouseWorldPos);
                evt.Consume();
            }
        }
    }

    public override void Serialise() {
        base.Serialise();
        
        // Hack to restore naked entity sprite
        entity.GetComponent<RenderComponent>().Graphics.SetSprite(TEXTURES.KEEPER);
    }
}