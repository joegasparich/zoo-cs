using Newtonsoft.Json;
using Zoo.entities;

namespace Zoo.ai; 

public class SleepBehaviour : Behaviour {
    // Constants
    private const float EnergyGainPerTick = 0.0001f; // TODO: Math this to be 8 hours once we have time
    private const int   CheckWakeInterval = 600; // 10 seconds
    
    public NeedsComponent Needs => actor.GetComponent<NeedsComponent>();

    [JsonConstructor]
    public SleepBehaviour() {}
    public SleepBehaviour(Actor actor) : base(actor) {}

    public override IEnumerable<Step> GetSteps() {
        var wait = Steps_General.Wait(CheckWakeInterval);
        wait.Update = () => {
            Needs.ModifyNeed(NeedDefOf.Energy.Id, EnergyGainPerTick);
        };
        wait.SuccessConditions.Add(() => Needs.Needs[NeedDefOf.Energy.Id].Full);
        
        yield return wait;
    }
}