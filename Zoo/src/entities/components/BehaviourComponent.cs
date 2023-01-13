using Zoo.ai;

namespace Zoo.entities; 

public class BehaviourComponent : Component {
    private Behaviour? currentBehaviour;

    protected override Type[] Dependencies => new[] { typeof(PathFollowComponent) };

    public BehaviourComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Start() {
        base.Start();

        if (entity is Actor actor) {
            SetBehaviour(new ConsumeBehaviour(actor));
        } else {
            Debug.Assert(false, "Non actor entity has behaviour component");
        }
    }

    public override void Update() {
        currentBehaviour?.Update();
    }
    
    public void SetBehaviour(Behaviour? behaviour) {
        currentBehaviour?.End();
        currentBehaviour = behaviour;
        currentBehaviour?.Start();
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveDeep("currentBehaviour", currentBehaviour);
    }
}