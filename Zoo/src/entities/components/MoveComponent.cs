namespace Zoo.entities; 

public class MoveComponent : Component {
    // Config
    public float Acceleration = 0.5f;

    // Properties
    protected override Type[] Dependencies => new Type[] { typeof(InputComponent), typeof(PhysicsComponent) };
    private InputComponent Input => entity.GetComponent<InputComponent>();
    private PhysicsComponent Physics => entity.GetComponent<PhysicsComponent>();

    public MoveComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Update() {
        Physics.AddForce(Input.InputVector * Acceleration);
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("acceleration", ref Acceleration);
    }
}