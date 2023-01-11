namespace Zoo.entities;

public class ElevationComponent : Component {
    // References
    private RenderComponent renderer;

    // Properties 
    protected override Type[] Dependencies => new Type[] { typeof(RenderComponent) };

    public ElevationComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        renderer = entity.GetComponent<RenderComponent>();
    }

    public override void Update() {
        renderer.Offset = renderer.Offset with{ Y = -Find.World.Elevation.GetElevationAtPos(entity.Pos) };
    }
}