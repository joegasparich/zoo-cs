namespace Zoo.entities;

public class ElevationComponent : Component {
    // Properties
    protected override Type[] Dependencies => new Type[] { typeof(RenderComponent) };
    private RenderComponent Renderer => entity.GetComponent<RenderComponent>();

    public ElevationComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Update() {
        Renderer.Offset = Renderer.Offset with{ Y = -Find.World.Elevation.GetElevationAtPos(entity.Pos) };
    }
}