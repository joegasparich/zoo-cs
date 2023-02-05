using Newtonsoft.Json.Linq;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world;

public class ExhibitManager : ISerialisable {
    // Collections
    private Dictionary<string, Exhibit> exhibits         = new();
    private Dictionary<string, Exhibit> exhibitsByArea   = new();
    private Dictionary<string, int>     reservedExhibits = new();

    // Listeners
    private string areaCreatedListener;
    private string areaUpdatedListener;
    private string areaRemovedListener;
    private string animalPlacedListener;
    private string animalRemovedListener;
    
    // Properties
    public IEnumerable<Exhibit> Exhibits => exhibits.Values;

    public void Setup() {
        areaCreatedListener   = Messenger.On(EventType.AreaCreated, OnAreaCreated);
        areaUpdatedListener   = Messenger.On(EventType.AreaUpdated, OnAreaUpdated);
        areaRemovedListener   = Messenger.On(EventType.AreaRemoved, OnAreaRemoved);
        animalPlacedListener  = Messenger.On(EventType.AnimalPlaced, OnAnimalPlaced);
        animalRemovedListener = Messenger.On(EventType.AnimalRemoved, OnAnimalRemoved);
    }

    public void Reset() {
        exhibits.Clear();
        exhibitsByArea.Clear();
        
        Messenger.Off(EventType.AreaCreated, areaCreatedListener);
        Messenger.Off(EventType.AreaUpdated, areaUpdatedListener);
        Messenger.Off(EventType.AreaRemoved, areaRemovedListener);
        Messenger.Off(EventType.AnimalPlaced, animalPlacedListener);
        Messenger.Off(EventType.AnimalRemoved, animalRemovedListener);
    }

    public Exhibit? RegisterExhibit(Area area) {
        if (area.IsZooArea) return null;

        var id   = Guid.NewGuid().ToString();
        var name = Guid.NewGuid().ToString(); // TODO: Generate a cool name
        var exhibit = new Exhibit(id, name, area);
        
        exhibits.Add(id, exhibit);
        exhibitsByArea.Add(area.Id, exhibit);
        
        Debug.Log($"Registered exhibit with name: {name}");

        return exhibit;
    }
    
    public void UnregisterExhibit(Exhibit exhibit) {
        if (!exhibits.ContainsKey(exhibit.Id)) return;
        
        exhibits.Remove(exhibit.Id);
        exhibitsByArea.Remove(exhibit.Area.Id);

        exhibit.Exists = false;
        
        Debug.Log($"Exhibit {exhibit.Name} unregistered");
    }
    
    public Exhibit GetExhibitById(string id) {
        if (!exhibits.ContainsKey(id)) return null;
        
        return exhibits[id];
    }

    public Exhibit? GetExhibitByArea(Area area) {
        if (!exhibitsByArea.ContainsKey(area.Id)) return null;
        
        return exhibitsByArea[area.Id];
    }
    
    public void UpdateAllExhibitCaches() {
        foreach (var exhibit in exhibits.Values) {
            exhibit.UpdateCache();
        }
    }

    public bool TryReserveExhibit(Exhibit exhibit, Actor actor) {
        if (reservedExhibits.ContainsKey(exhibit.Id))
            return false;

        reservedExhibits.Add(exhibit.Id, actor.Id);
        return true;
    }
    public void UnreserveExhibit(Exhibit exhibit) {
        if (!reservedExhibits.ContainsKey(exhibit.Id)) return;

        reservedExhibits.Remove(exhibit.Id);
    }
    public bool IsExhibitReserved(Exhibit exhibit) {
        return reservedExhibits.ContainsKey(exhibit.Id);
    }

    private void OnAreaCreated(object obj) {
        var area = obj as Area;
        
        if (area.GetContainedEntities(EntityTag.Animal).Any()) {
            RegisterExhibit(area);
        }
    }
    
    private void OnAreaUpdated(object obj) {
        var area = obj as Area;
        
        if (exhibitsByArea.ContainsKey(area.Id)) {
            var exhibit = exhibitsByArea[area.Id];
            
            if (area.GetContainedEntities(EntityTag.Animal).Any())
                exhibit.UpdateCache();
            else {
                UnregisterExhibit(exhibit);
            }
        } else if (area.GetContainedEntities(EntityTag.Animal).Any()) {
            RegisterExhibit(area);
        }
    }

    private void OnAreaRemoved(object obj) {
        var area = obj as Area;

        var exhibit = GetExhibitByArea(area);
        if (exhibit != null)
            UnregisterExhibit(exhibit);
    }

    private void OnAnimalPlaced(object obj) {
        var animal = obj as Animal;

        if (exhibitsByArea.ContainsKey(animal.Area.Id)) {
            animal.Exhibit.UpdateCache();
            return;
        }
        
        if (!animal.Area.IsZooArea) {
            RegisterExhibit(animal.Area);
        }
        
        animal.Exhibit?.UpdateCache();
    }

    private void OnAnimalRemoved(object obj) {
        var animal = obj as Animal;

        if (animal.Exhibit != null) {
            animal.Exhibit.UpdateCache();

            if (animal.Exhibit.ContainedAnimals.NullOrEmpty()) {
                UnregisterExhibit(animal.Exhibit);
            }
        }
    }

    public void Serialise() {
        Find.SaveManager.ArchiveCollection("exhibits", exhibits.Values, data => {
            foreach (JObject node in data) {
                exhibits.Add(node["id"].Value<string>(), new Exhibit());
            }
            return exhibits.Values;
        });

        Find.SaveManager.ArchiveValue("reservedExhibits", () => reservedExhibits.Select(kv => (kv.Key, kv.Value)), t => {
            foreach (var (exhibitId, actorId) in t) {
                reservedExhibits.Add(exhibitId, actorId);
            }
        });

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            foreach(var exhibit in exhibits.Values) {
                exhibitsByArea.Add(exhibit.Area.Id, exhibit);
            }
        }
    }
}