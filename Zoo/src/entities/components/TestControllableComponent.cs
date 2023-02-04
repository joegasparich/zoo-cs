using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.entities; 

public class TestControllableComponent : Component {
    protected override Type[] Dependencies => new[] { typeof(PathFollowComponent) };
    private PathFollowComponent Pathfinder => entity.GetComponent<PathFollowComponent>();

    public TestControllableComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Render() {
        var path = Pathfinder.GetPath();
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
                Pathfinder.PathTo(evt.mouseWorldPos);
                evt.Consume();
            }
        }
    }
}