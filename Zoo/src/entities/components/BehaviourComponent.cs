using Zoo.ai;

namespace Zoo.entities; 

public class BehaviourComponent : Component {
    private Behaviour? currentBehaviour;

    protected override Type[] Dependencies => new[] { typeof(PathFollowComponent) };

    public BehaviourComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Start() {
        base.Start();

        SetBehaviour(new IdleBehaviour(entity));
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