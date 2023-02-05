using Zoo.util;

namespace Zoo.world;

public interface IBlueprintable {
    public bool IsBlueprint { get; }
    public string BlueprintId { get; }

    public void BuildBlueprint();
    public List<IntVec2> GetBuildTiles();
}