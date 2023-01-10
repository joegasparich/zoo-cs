namespace Zoo.entities; 

public class MoveComponent : Component {
    // References
    private InputComponent   input;
    private PhysicsComponent physics;

    // Config
    public float Acceleration = 0.5f;

    // Properties
    protected override Type[] Dependencies => new Type[] { typeof(InputComponent), typeof(PhysicsComponent) };

    public MoveComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        input = entity.GetComponent<InputComponent>();
        physics = entity.GetComponent<PhysicsComponent>();
    }

    public override void Update() {
        physics.AddForce(input.InputVector * Acceleration);
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("acceleration", ref Acceleration);
    }
}