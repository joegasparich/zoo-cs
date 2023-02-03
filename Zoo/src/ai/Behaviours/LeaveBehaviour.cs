using Zoo.entities;

namespace Zoo.ai; 

public class LeaveBehaviour : Behaviour {
    
    public LeaveBehaviour() {}
    public LeaveBehaviour(Actor actor) : base(actor) {}

    public override IEnumerable<Step> GetSteps() {
        yield return Steps_General.GoTo(Find.Zoo.Entrance);
        
        yield return Steps_General.Do(() => {
            actor.Destroy();
        });
    }
}