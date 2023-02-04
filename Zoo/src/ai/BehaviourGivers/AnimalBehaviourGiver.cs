using Zoo.entities;

namespace Zoo.ai; 

public static class AnimalBehaviourGiver {
    // Modifiers for the priority of the behaviour
    public static readonly float ThirstPriority  = 0.75f;
    public static readonly float HungerPriority  = 0.65f;
    public static readonly float EnergyPriorityAwake  = 0.3f;
    public static readonly float EnergyPriorityAsleep  = 2.0f;
    
    // Thresholds where entity will start prioritising the behaviour
    public static readonly float HungerThreshold = 0.5f;
    public static readonly float ThirstThreshold = 0.5f;
    public static readonly float EnergyThreshold = 0.2f;
    
    public static Behaviour Get(Animal animal) {
        var priorityQueue = new PriorityQueue<Func<Behaviour>, float>(Comparer<float>.Create((x, y) => Math.Sign(y - x)));
        
        // TODO: Is there a better factoring for this? I kind of hate it but I don't want a JobGiver equivalent
        
        // Thirst
        if (animal.GetNeed(NeedDefOf.Thirst).Value < ThirstThreshold) {
            if (animal.Area.GetContainedEntities(EntityTag.Consumable).Any(e => e.GetComponent<ConsumableComponent>().Data.Need.Def == NeedDefOf.Thirst)) {
                priorityQueue.Enqueue(() => new ConsumeBehaviour(animal, NeedDefOf.Thirst), (1 - animal.GetNeed(NeedDefOf.Thirst).Value) * ThirstPriority);
                animal.MissingWaterSource = false;
            } else {
                animal.MissingWaterSource = true;
            }

        }

        // Hunger
        if (animal.GetNeed(NeedDefOf.Hunger).Value < HungerThreshold) {
            if (animal.Area.GetContainedEntities(EntityTag.Consumable).Any(e => e.GetComponent<ConsumableComponent>().Data.Need.Def == NeedDefOf.Hunger)) {
                priorityQueue.Enqueue(() => new ConsumeBehaviour(animal, NeedDefOf.Hunger), (1 - animal.GetNeed(NeedDefOf.Hunger).Value) * HungerPriority);
                animal.MissingFoodSource = false;
            } else {
                animal.MissingFoodSource = true;
            }
        }
        
        // Energy
        var energyPriority = animal.IsAsleep ?  EnergyPriorityAsleep : EnergyPriorityAwake;
        if (animal.IsAsleep || animal.GetNeed(NeedDefOf.Energy).Value < EnergyThreshold) {
            priorityQueue.Enqueue(() => new SleepBehaviour(animal), (1  - animal.GetNeed(NeedDefOf.Energy).Value) * energyPriority);
        }
        
        priorityQueue.Enqueue(() => new IdleBehaviour(animal), 0f);
        
        return priorityQueue.Dequeue()();
    }
}