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
    public static Type DataType => typeof(BehaviourComponentData);

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
        if (currentBehaviour is { State: CompleteState.Expired }) {
            currentBehaviour.OnExpire();
        }
        // Check for completed
        if (currentBehaviour is { State: CompleteState.Success or CompleteState.Failed or CompleteState.Interrupted }) {
            currentBehaviour.OnComplete();
        }
        
        // Get new behaviour if completed
        if (currentBehaviour is null or not { State: CompleteState.Incomplete })
            SetBehaviour(GetNewBehaviour());
        
        currentBehaviour?.Update();
    }

    private Behaviour GetNewBehaviour() {
        return (Behaviour)Data.BehaviourGiverClass.GetMethod("Get")?.Invoke(null, new object[] { Actor })!;
    }
    
    public void SetBehaviour(Behaviour? behaviour) {
        currentBehaviour?.End();
        currentBehaviour = behaviour;
        currentBehaviour?.Start();
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveDeep("currentBehaviour", () => currentBehaviour, savedBehaviour => currentBehaviour = savedBehaviour as Behaviour);
    }
}
