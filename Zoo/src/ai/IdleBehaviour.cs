using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai; 

public class IdleBehaviour : Behaviour {
    // State
    private IntVec2?            wanderTile;

    // Properties
    public PathFollowComponent Pather => actor.GetComponent<PathFollowComponent>();

    public IdleBehaviour() {}
    public IdleBehaviour(Actor actor) : base(actor) {}

    public override void Update() {
        base.Update();

        if (wanderTile == null || !Pather.HasPath) {
            wanderTile = LocationUtility.RandomWalkableCellInAreaInRadius(actor.Area, actor.Pos.Floor(), 5, actor.Accessibility);

            if (wanderTile.HasValue) {
                if (!Pather.PathTo(wanderTile.Value)) 
                    wanderTile = null;
            }
        }
        
        if (Pather.ReachedDestination()) {
            wanderTile = null;
        }
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("wanderTile", ref wanderTile);
    }
}