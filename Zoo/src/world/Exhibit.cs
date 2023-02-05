using Newtonsoft.Json;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world; 


public class Exhibit : ISerialisable {
    // Constants
    private const int MaintenanceCooldown = 300; // 5 seconds

    // State
    public string Id;
    public string Name;
    public Area   Area;
    public bool   Exists         = true;
    public int    LastMaintainedTick = -99999;
    
    // Cache
    public HashSet<IntVec2> ViewingTiles     = new();
    public List<Animal>     ContainedAnimals = new();
    
    // Properties
    public bool Reserved         => Find.World.Exhibits.IsExhibitReserved(this);
    public bool NeedsMaintenance => (MissingFood || MissingWater) && LastMaintainedTick + MaintenanceCooldown < Game.Ticks;
    public bool MissingFood      => ContainedAnimals.Any(animal => animal.MissingFoodSource);
    public bool MissingWater     => ContainedAnimals.Any(animal => animal.MissingWaterSource);

    [JsonConstructor]
    public Exhibit() {}
    public Exhibit(string id, string name, Area area) {
        Id   = id;
        Name = name;
        Area = area;
        
        UpdateCache();
    }

    public void UpdateCache() {
        ViewingTiles.Clear();
        ContainedAnimals.Clear();

        ContainedAnimals = Area.GetContainedEntities(EntityTag.Animal).Cast<Animal>().ToList();

        foreach (var tile in Area.Tiles) {
            foreach (var neighbour in Find.World.GetAdjacentTiles(tile)) {
                if (neighbour.GetArea().IsZooArea) {
                    ViewingTiles.Add(neighbour);
                }
            }
        }
    }
    
    public void Serialise() {
        Find.SaveManager.ArchiveValue("id",       ref Id);
        Find.SaveManager.ArchiveValue("name",     ref Name);
        Find.SaveManager.ArchiveValue("areaTile", () => Area.Tiles[0], tile => Area = Find.World.Areas.GetAreaAtTile(tile));
        Find.SaveManager.ArchiveValue("lastMaintained",   ref LastMaintainedTick);
    }
}