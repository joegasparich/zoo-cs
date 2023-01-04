namespace Zoo.entities; 

public class MoveComponent : Component {
    private InputComponent   input;
    private PhysicsComponent physics;

    public float Acceleration = 0.5f;

    protected override Type[] Dependencies => new Type[] { typeof(InputComponent), typeof(PhysicsComponent) };

    public MoveComponent(Entity entity) : base(entity) {}

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