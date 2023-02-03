using System.Runtime.Serialization;
using Newtonsoft.Json;
using Zoo.ai;

namespace Zoo.entities;

public class BehaviourComponentData : ComponentData {
    public override Type CompClass => typeof(BehaviourComponent);

    [JsonProperty]
    private string behaviourGiverClass;

    public Type BehaviourGiverClass;

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context) {
        BehaviourGiverClass = Type.GetType("Zoo.ai." + behaviourGiverClass);
    }
}

public class BehaviourComponent : Component {
    // State
    protected Behaviour? currentBehaviour;
    
    // Properties
    public             BehaviourComponentData Data             => (BehaviourComponentData)data;
    protected override Type[]                 Dependencies     => new[] { typeof(PathFollowComponent) };
    protected          Actor                  Actor            => entity as Actor;
    public             Behaviour              CurrentBehaviour => currentBehaviour;

    public BehaviourComponent(Entity entity, BehaviourComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        Debug.Assert(entity is Actor, "Non actor entity has behaviour component");
    }
    
    public override void Update() {
        // Check for expired
        if (currentBehaviour is { Expired: true }) {
            currentBehaviour.OnExpire();
            currentBehaviour = null;
        }
        // Check for completed
        if (currentBehaviour is { Completed: true }) {
            currentBehaviour.OnComplete();
            currentBehaviour = null;
        }
        
        // Get new behaviour
        if (currentBehaviour == null)
            SetBehaviour(GetNewBehaviour());
        
        currentBehaviour?.Update();
    }

    private Behaviour GetNewBehaviour() {
        return (Behaviour)Data.BehaviourGiverClass.GetMethod("Get")?.Invoke(null, new object[] { Actor })!;
    }
    
    public void SetBehaviour(Behaviour? behaviour) {
        currentBehaviour?.OnComplete();
        currentBehaviour = behaviour;
        currentBehaviour?.Start();
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveDeep("currentBehaviour", () => currentBehaviour, savedBehaviour => currentBehaviour = savedBehaviour as Behaviour);
    }
}
