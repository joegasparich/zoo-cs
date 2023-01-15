using Zoo.ai;
using Zoo.defs;

namespace Zoo.entities; 

public abstract class BehaviourComponent : Component {
    // State
    protected Behaviour? currentBehaviour;

    // Properties
    protected override Type[] Dependencies => new[] { typeof(PathFollowComponent) };
    protected Actor Actor => entity as Actor;
    public Behaviour CurrentBehaviour => currentBehaviour;

    public BehaviourComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Start() {
        base.Start();
        Debug.Assert(entity is Actor, "Non actor entity has behaviour component");

        SetBehaviour(new IdleBehaviour(Actor));
    }

    public override void Update() {
        // Check for expired
        if (currentBehaviour is { Expired: true }) {
            currentBehaviour.OnExpire();
            currentBehaviour = null;
        }
        // Check for completed
        if (currentBehaviour is { completed: true }) {
            currentBehaviour.OnComplete();
            currentBehaviour = null;
        }
        
        // Get new behaviour
        if (currentBehaviour == null)
            SetBehaviour(GetNewBehaviour());
        
        currentBehaviour?.Update();

    }

    protected abstract Behaviour GetNewBehaviour();
    
    public void SetBehaviour(Behaviour? behaviour) {
        currentBehaviour?.OnComplete();
        currentBehaviour = behaviour;
        currentBehaviour?.Start();
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveDeep("currentBehaviour", currentBehaviour);
    }
}