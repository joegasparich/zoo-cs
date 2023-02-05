using Newtonsoft.Json;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai;

public class BuildBlueprintBehaviour : Behaviour {
    // Constants
    private int BuildTicks = 60; // 1 second

    // State
    private TileObject blueprint;
    private IntVec2    closestTile;

    [JsonConstructor]
    public BuildBlueprintBehaviour() {}
    public BuildBlueprintBehaviour(Actor actor) : base(actor) {}

    public override void Start() {
        base.Start();

        blueprint   = Find.Zoo.ObjBlueprints.RandomElement();
        closestTile = blueprint.GetOccupiedTiles().Closest(actor.Pos);
    }

    public override IEnumerable<Step> GetSteps() {
        // Go to blueprint
        yield return Steps_General.GoTo(closestTile, PathMode.Adjacent)
            .FailOn(() => blueprint.Despawned)
            .FailOn(() => !blueprint.IsBlueprint);

        // Build for a while
        yield return Steps_General.Wait(BuildTicks)
            .FailOn(() => blueprint.Despawned)
            .FailOn(() => !blueprint.IsBlueprint);

        // Build blueprint
        yield return Steps_General.Do(() => {
            blueprint.BuildBlueprint();
        });
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("blueprint", () => blueprint.Id, id => blueprint = Game.GetEntityById(id) as TileObject);
        Find.SaveManager.ArchiveValue("closestTile", ref closestTile);
    }
}