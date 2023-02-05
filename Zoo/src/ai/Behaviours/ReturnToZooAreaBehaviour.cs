using Newtonsoft.Json;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai;

public class ReturnToZooAreaBehaviour : Behaviour {
    // State
    private IntVec2 targetTile;

    [JsonConstructor]
    public ReturnToZooAreaBehaviour() {}
    public ReturnToZooAreaBehaviour(Actor actor) : base(actor) {}

    public override void Start() {
        base.Start();

        var areaPath = Find.World.Areas.BFS(actor.Area, Find.World.Areas.ZooArea);

        if (areaPath.NullOrEmpty()) {
            State = CompleteState.Failed;
            return;
            // TODO: Prevent this behaviour from running for a while
        }

        areaPath.Last().ConnectedAreas.TryGetValue(areaPath[^2], out var doors);
        targetTile = doors.RandomElement().GetAdjacentTiles().Find(tile => tile.GetArea().IsZooArea);

        if (targetTile == null) {
            State = CompleteState.Failed;
            // TODO: Prevent this behaviour from running for a while
        }
    }

    public override IEnumerable<Step> GetSteps() {
        yield return Steps_General.GoTo(targetTile)
            .SucceedOn(() => actor.Area.IsZooArea);
    }
}