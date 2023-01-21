using Zoo.entities;

namespace Zoo.ai; 

public class LeaveBehaviour : Behaviour {
    
    public LeaveBehaviour() {}
    public LeaveBehaviour(Actor actor) : base(actor) {}

    public override void Update() {
        base.Update();
        
        // Reached entrance
        if (Pather.ReachedDestination) {
            actor.Destroy();
            completed = true;
            return;
        }

        if (Pather.NoPathFound) {
            completed = true;
            Debug.Log($"Guest {actor.Id} could not find path to zoo entrance");
        }
        
        if (!Pather.HasPath) {
            Pather.PathTo(Find.Zoo.Entrance);
        }
    }
}