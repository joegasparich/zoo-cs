using Newtonsoft.Json.Linq;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world;

public class Exhibit : ISerialisable {
    // State
    public string Name;
    public Area   Area;
    
    // Cache
    public HashSet<IntVec2> ViewingTiles     = new();
    public List<Animal>     ContainedAnimals = new();

    public Exhibit() {}
    public Exhibit(string name, Area area) {
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
        Find.SaveManager.ArchiveValue("name",     ref Name);
        Find.SaveManager.ArchiveValue("areaTile", () => Area.Tiles[0], tile => Area = Find.World.Areas.GetAreaAtTile(tile));
    }
}

public class ExhibitManager : ISerialisable {
    // Collections
    private Dictionary<string, Exhibit> exhibits = new();

    // Listeners
    private string areaCreatedListener;
    private string areaUpdatedListener;
    private string areaRemovedListener;
    private string animalPlacedListener;
    private string animalRemovedListener;

    public void Setup() {
        areaCreatedListener   = Messenger.On(EventType.AreaCreated, OnAreaCreated);
        areaUpdatedListener   = Messenger.On(EventType.AreaUpdated, OnAreaUpdated);
        areaRemovedListener   = Messenger.On(EventType.AreaRemoved, OnAreaRemoved);
        animalPlacedListener  = Messenger.On(EventType.AnimalPlaced, OnAnimalPlaced);
        animalRemovedListener = Messenger.On(EventType.AnimalRemoved, OnAnimalRemoved);
    }

    public void Reset() {
        exhibits.Clear();
        
        Messenger.Off(EventType.AreaCreated, areaCreatedListener);
        Messenger.Off(EventType.AreaUpdated, areaUpdatedListener);
        Messenger.Off(EventType.AreaRemoved, areaRemovedListener);
        Messenger.Off(EventType.AnimalPlaced, animalPlacedListener);
        Messenger.Off(EventType.AnimalRemoved, animalRemovedListener);
    }

    public Exhibit? RegisterExhibit(Area area) {
        if (area.IsZooArea) return null;
        
        var name = Guid.NewGuid().ToString(); // TODO: Generate a cool name
        exhibits.Add(area.Id, new Exhibit(name, area));

        return exhibits[area.Id];
    }
    
    public void UnregisterExhibit(Exhibit exhibit) {
        if (!exhibits.ContainsKey(exhibit.Area.Id)) return;
        
        exhibits.Remove(exhibit.Area.Id);
    }

    public Exhibit? GetExhibitByArea(Area area) {
        if (!exhibits.ContainsKey(area.Id)) return null;
        
        return exhibits[area.Id];
    }
    
    public void UpdateAllExhibitCaches() {
        foreach (var exhibit in exhibits.Values) {
            exhibit.UpdateCache();
        }
    }

    private void OnAreaCreated(object obj) {
        var area = obj as Area;
        
        if (area.GetContainedEntities(EntityTag.Animal).Any()) {
            RegisterExhibit(area);
        }
    }
    
    private void OnAreaUpdated(object obj) {
        var area = obj as Area;
        
        if (exhibits.ContainsKey(area.Id)) {
            if (area.GetContainedEntities(EntityTag.Animal).Any())
                exhibits[area.Id].UpdateCache();
            else {
                exhibits.Remove(area.Id);
            }
        } else if (area.GetContainedEntities(EntityTag.Animal).Any()) {
            RegisterExhibit(area);
        }
    }

    private void OnAreaRemoved(object obj) {
        var area = obj as Area;
        
        UnregisterExhibit(GetExhibitByArea(area));
    }

    private void OnAnimalPlaced(object obj) {
        var animal = obj as Animal;

        if (exhibits.ContainsKey(animal.Area.Id)) {
            animal.Exhibit.UpdateCache();
            return;
        }
        
        if (!animal.Area.IsZooArea) {
            RegisterExhibit(animal.Area);
        }
        
        animal.Exhibit.UpdateCache();
    }

    private void OnAnimalRemoved(object obj) {
        var animal = obj as Animal;
        
        animal.Exhibit.UpdateCache();

        if (animal.Exhibit.ContainedAnimals.NullOrEmpty()) {
            UnregisterExhibit(animal.Exhibit);
        }
    }

    public void Serialise() {
        Find.SaveManager.ArchiveCollection("exhibits", exhibits.Values, data => {
            foreach (JObject node in data) {
                var area = Find.World.Areas.GetAreaAtTile(node["areaTile"].ToObject<IntVec2>());
                exhibits.Add(area.Id, new Exhibit());
            }
            return exhibits.Values;
        });
    }
}