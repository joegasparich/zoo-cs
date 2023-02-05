using Zoo.entities;

namespace Zoo.world;

public class BlueprintManager : ISerialisable {
    public  Dictionary<string, IBlueprintable> Blueprints { get; } = new();
    private Dictionary<string, int>            reservedBlueprints = new();

    public void RegisterBlueprint(IBlueprintable blueprint) {
        Blueprints.Add(blueprint.BlueprintId, blueprint);
    }
    public void UnregisterBlueprint(IBlueprintable blueprint) {
        Blueprints.Remove(blueprint.BlueprintId);
    }

    public bool TryReserveBlueprint(IBlueprintable blueprint, Actor actor) {
        if (reservedBlueprints.ContainsKey(blueprint.BlueprintId))
            return false;

        reservedBlueprints.Add(blueprint.BlueprintId, actor.Id);
        return true;
    }
    public void UnreserveBlueprint(IBlueprintable blueprint) {
        if (!reservedBlueprints.ContainsKey(blueprint.BlueprintId)) return;

        reservedBlueprints.Remove(blueprint.BlueprintId);
    }
    public bool IsBlueprintReserved(IBlueprintable blueprint) {
        return reservedBlueprints.ContainsKey(blueprint.BlueprintId);
    }

    public void Serialise() {
        Find.SaveManager.ArchiveValue("reservedBlueprints", () => reservedBlueprints.Select(kv => (kv.Key, kv.Value)), t => {
            foreach (var (blueprintId, actorId) in t) {
                reservedBlueprints.Add(blueprintId, actorId);
            }
        });
    }
}