namespace Zoo.entities; 

public class MoveComponent : Component {
    private InputComponent   input;
    private PhysicsComponent physics;

    public float Acceleration { get; set; } = 0.5f;

    public MoveComponent(Entity entity) : base(entity) {}

    public override void Start() {
        // TODO: add back requried components
        input = entity.GetComponent<InputComponent>();
        physics = entity.GetComponent<PhysicsComponent>();
    }

    public override void Update() {
        physics.AddForce(input.InputVector * Acceleration);
    }
}