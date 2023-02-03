using System.Numerics;
using Zoo.defs;
using Zoo.world;

namespace Zoo.entities; 

public class Staff : Actor {
    // Constants
    private const int ExhibitAccessibilityCacheInterval = 600; // 10 seconds
    
    // Cache
    public HashSet<Exhibit> AccesibleExhibits = new ();
    
    // Properties
    public PersonComponent Person => GetComponent<PersonComponent>();
    
    public Staff(Vector2 pos, ActorDef? def) : base(pos, def) {}
    
    public override void Setup() {
        base.Setup();
        
        Find.Zoo.Staff.Add(this);
    }

    public override void Update() {
        base.Update();
        
        if (Game.Ticks % ExhibitAccessibilityCacheInterval == 0) {
            UpdateAccesibleExhibitsCache();
        }
    }

    public override void Destroy() {
        Find.Zoo.Staff.Remove(this);
        
        base.Destroy();
    }

    private void UpdateAccesibleExhibitsCache() {
        AccesibleExhibits.Clear();
        foreach (var exhibit in Find.World.Exhibits.Exhibits) {
            if (exhibit.Area.ReachableFrom(Area)) {
                AccesibleExhibits.Add(exhibit);
            }
        }
    }
}