﻿using Newtonsoft.Json.Linq;
using Zoo.entities;
using Zoo.util;

namespace Zoo.world;

public class Exhibit : ISerialisable {
    // State
    public string Id;
    public string Name;
    public Area   Area;
    
    // Cache
    public HashSet<IntVec2> ViewingTiles     = new();
    public List<Animal>     ContainedAnimals = new();

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

public class ExhibitManager : ISerialisable {
    // Collections
    private Dictionary<string, Exhibit> exhibits = new();
    private Dictionary<string, Exhibit> exhibitsByArea = new();

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
        
        Debug.Log($"Exhibit {exhibit.Name} unregistered");
    }
    
    public Exhibit GetExhibitById(string areaId) {
        if (!exhibits.ContainsKey(areaId)) return null;
        
        return exhibits[areaId];
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
        
        UnregisterExhibit(GetExhibitByArea(area));
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
                exhibits.Add(node["id"].Value<string>(), new Exhibit());
            }
            return exhibits.Values;
        });

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            foreach(var exhibit in exhibits.Values) {
                exhibitsByArea.Add(exhibit.Area.Id, exhibit);
            }
        }
    }
}