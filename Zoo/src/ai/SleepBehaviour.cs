using Zoo.entities;

namespace Zoo.ai; 

public class SleepBehaviour : Behaviour {
    // Constants
    private const float EnergyGainPerTick = 0.0001f; // TODO: Math this to be 8 hours once we have time
    
    public NeedsComponent Needs => actor.GetComponent<NeedsComponent>();

    public SleepBehaviour() {}
    public SleepBehaviour(Actor actor) : base(actor) {}

    public override void Update() {
        base.Update();
        
        Needs.ModifyNeed(NeedDefOf.Energy.Id, EnergyGainPerTick);
        
        if (Needs.Needs[NeedDefOf.Energy.Id].Full) {
            completed = true;
            return;
        }
        
    }
}