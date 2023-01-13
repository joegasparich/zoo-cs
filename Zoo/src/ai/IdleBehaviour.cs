using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai; 

public class IdleBehaviour : Behaviour {
    // State
    private IntVec2?            wanderTile;

    // Properties
    public PathFollowComponent Pather => entity.GetComponent<PathFollowComponent>();
    public Actor               Actor  => entity as Actor;

    public IdleBehaviour() {}
    public IdleBehaviour(Entity entity) : base(entity) {}

    public override void Start() {
        Debug.Assert(Actor != null);
        base.Start();
    }

    public override void Update() {
        base.Update();

        if (wanderTile == null || !Pather.HasPath) {
            wanderTile = LocationUtility.RandomWalkableCellInAreaInRadius(Actor.Area, Actor.Pos.Floor(), 5, Actor.Accessibility);

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