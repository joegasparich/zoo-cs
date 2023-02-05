using Zoo.util;

namespace Zoo.world;

public interface IBlueprintable {
    // Properties
    public bool   IsBlueprint { get; }
    public string BlueprintId { get; }
    public bool   Reserved    => Find.World.Blueprints.IsBlueprintReserved(this);

    public void BuildBlueprint();
    public List<IntVec2> GetBuildTiles();
}