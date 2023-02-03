using Zoo.entities;
using Zoo.util;

namespace Zoo.world; 


public class Exhibit : ISerialisable {
    // State
    public string Id;
    public string Name;
    public Area   Area;
    public bool   Exists = true;
    
    // Cache
    public HashSet<IntVec2> ViewingTiles     = new();
    public List<Animal>     ContainedAnimals = new();
    
    // Properties
    public bool MissingFood => ContainedAnimals.Any(animal => animal.MissingFoodSource);
    public bool MissingWater => ContainedAnimals.Any(animal => animal.MissingWaterSource);

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
    }
}