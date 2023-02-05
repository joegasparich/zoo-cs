using Newtonsoft.Json;
using Zoo.entities;

namespace Zoo.ai; 

public class LeaveBehaviour : Behaviour {
    [JsonConstructor]
    public LeaveBehaviour() {}
    public LeaveBehaviour(Actor actor) : base(actor) {}

    public override IEnumerable<Step> GetSteps() {
        yield return Steps_General.GoTo(Find.Zoo.Entrance);
        
        yield return Steps_General.Do(() => {
            actor.Destroy();
        });
    }
}