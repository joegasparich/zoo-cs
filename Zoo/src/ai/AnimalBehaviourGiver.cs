using Zoo.entities;

namespace Zoo.ai; 

public static class AnimalBehaviourGiver {
    public static readonly float ThirstPriority  = 0.5f;
    public static readonly float ThirstThreshold = 0.5f;
    public static readonly float HungerPriority  = 0.75f;
    public static readonly float HungerThreshold = 0.5f;
    public static readonly float EnergyPriority  = 0.3f;
    public static readonly float EnergyThreshold = 0.2f;
    
    public static Behaviour Get(Animal animal) {
        var priorityQueue = new PriorityQueue<Func<Behaviour>, float>(Comparer<float>.Create((x, y) => Math.Sign(y - x)));
        
        // TODO: Is there a better factoring for this? I kind of hate it but I don't want a JobGiver equivalent
        
        // Thirst
        if (animal.GetNeed(NeedDefOf.Thirst).Value < ThirstThreshold 
         && animal.Area.GetContainedEntities(EntityTag.Consumable).Any(e => e.GetComponent<ConsumableComponent>().Data.Need.Def == NeedDefOf.Thirst)) {
            priorityQueue.Enqueue(() => new ConsumeBehaviour(animal, NeedDefOf.Thirst), (1 - animal.GetNeed(NeedDefOf.Thirst).Value) * ThirstPriority);
        }
        // Hunger
        if (animal.GetNeed(NeedDefOf.Hunger).Value < HungerThreshold 
         && animal.Area.GetContainedEntities(EntityTag.Consumable).Any(e => e.GetComponent<ConsumableComponent>().Data.Need.Def == NeedDefOf.Hunger)) {
            priorityQueue.Enqueue(() => new ConsumeBehaviour(animal, NeedDefOf.Hunger), (1  - animal.GetNeed(NeedDefOf.Hunger).Value) * HungerPriority);
        }
        // Energy
        if (animal.GetNeed(NeedDefOf.Energy).Value < EnergyThreshold) {
            priorityQueue.Enqueue(() => new SleepBehaviour(animal), (1  - animal.GetNeed(NeedDefOf.Energy).Value) * EnergyPriority);
        }
        
        priorityQueue.Enqueue(() => new IdleBehaviour(animal), 0f);
        
        return priorityQueue.Dequeue()();
    }
}