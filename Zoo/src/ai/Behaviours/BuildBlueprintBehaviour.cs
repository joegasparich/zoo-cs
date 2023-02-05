using Newtonsoft.Json;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai;

public class BuildBlueprintBehaviour : Behaviour {
    // Constants
    private int BuildTicks = 60; // 1 second

    // State
    private IBlueprintable blueprint;
    private IntVec2        closestTile;

    [JsonConstructor]
    public BuildBlueprintBehaviour() {}
    public BuildBlueprintBehaviour(Actor actor) : base(actor) {}

    public override void Start() {
        base.Start();

        blueprint = Find.World.Blueprints.Blueprints.Values.Where(bp => !bp.Reserved).RandomElement();

        if (!Find.World.Blueprints.TryReserveBlueprint(blueprint, actor)) {
            State = CompleteState.Failed;
            return;
        }

        closestTile = blueprint.GetBuildTiles().Closest(actor.Pos);
    }

    public override void End() {
        Find.World.Blueprints.UnreserveBlueprint(blueprint);
    }

    public override IEnumerable<Step> GetSteps() {
        // Go to blueprint
        yield return Steps_General.GoTo(closestTile)
            .FailOn(() => blueprint is Entity e && e.Despawned)
            .FailOn(() => !blueprint.IsBlueprint);

        // Build for a while
        yield return Steps_General.Wait(BuildTicks)
            .FailOn(() => blueprint is Entity e && e.Despawned)
            .FailOn(() => !blueprint.IsBlueprint);

        // Build blueprint
        yield return Steps_General.Do(() => {
            blueprint.BuildBlueprint();
        });
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("blueprint",   () => blueprint.BlueprintId, id => blueprint = Find.World.Blueprints.Blueprints[id]);
        Find.SaveManager.ArchiveValue("closestTile", ref closestTile);
    }
}