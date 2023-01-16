using Zoo.ai;
using Zoo.defs;

namespace Zoo.entities; 

public class AnimalBehaviourComponent : BehaviourComponent {
    // Properties
    protected override Type[]         Dependencies => new[] { typeof(PathFollowComponent), typeof(NeedsComponent) };
    protected          Animal         Animal       => entity as Animal;
    protected          NeedsComponent Needs        => Animal.GetComponent<NeedsComponent>();
    
    public AnimalBehaviourComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        Debug.Assert(entity is Animal);
    }

    protected override Behaviour GetNewBehaviour() {
        return AnimalBehaviourGiver.Get(Animal);
    }
}