using Zoo.ai;
using Zoo.defs;

namespace Zoo.entities; 

public class AnimalBehaviourComponent : BehaviourComponent {
    // Properties
    protected override Type[]         Dependencies     => new[] { typeof(PathFollowComponent), typeof(NeedsComponent) };
    protected          Actor          Animal           => entity as Animal;
    protected          NeedsComponent Needs            => Animal.GetComponent<NeedsComponent>();
    
    public AnimalBehaviourComponent(Entity entity, ComponentData? data = null) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        Debug.Assert(entity is Animal);
    }

    protected override Behaviour GetNewBehaviour() {
        // TODO: Order needs by priority
        // TODO: Needs provide the behaviour to make this more scalable
        foreach (var need in Needs.Needs.Values) {
            if (need.Value < 0.5f) {
                if (need.Def.Def == NeedDefOf.Hunger || need.Def.Def == NeedDefOf.Thirst)
                    return new ConsumeBehaviour(Animal, need.Def);
                if (need.Def.Def == NeedDefOf.Energy)
                    return new SleepBehaviour(Animal);
            }
        }
        
        return new IdleBehaviour(Animal);
    }
}