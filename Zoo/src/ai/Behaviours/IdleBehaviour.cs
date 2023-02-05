using Newtonsoft.Json;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai;

public class IdleBehaviour : Behaviour {
    // State
    private IntVec2? wanderTile;

    // Properties
    public PathFollowComponent Pather => actor.GetComponent<PathFollowComponent>();

    [JsonConstructor]
    public IdleBehaviour() {}
    public IdleBehaviour(Actor actor) : base(actor) {}

    public override void Start() {
        base.Start();

        wanderTile = LocationUtility.RandomWalkableCellInAreaInRadius(actor.Pos.Floor(), 5, actor.Area, actor.Accessibility);
    }

    public override IEnumerable<Step> GetSteps() {
        if (!wanderTile.HasValue)
            yield break;
        
        yield return Steps_General.GoTo(wanderTile.Value);
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("wanderTile", ref wanderTile);
    }
}