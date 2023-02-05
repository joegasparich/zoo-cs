using System.Numerics;
using Zoo.defs;
using Zoo.world;

namespace Zoo.entities; 

public class Guest : Actor {
    // State
    public HashSet<Exhibit> ExhibitsViewed = new ();
    
    // Properties
    public          PersonComponent Person => GetComponent<PersonComponent>();
    public override string          Name   => Person.FullName;

    public Guest(Vector2 pos, ActorDef? def) : base(pos, def) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        Find.Zoo.Guests.Add(this);
    }

    public override void Destroy() {
        Find.Zoo.Guests.Remove(this);
        
        base.Destroy();
    }

    public override void Serialise() {
        base.Serialise();
        
        // TODO: Replace with ArchiveReferences
        Find.SaveManager.ArchiveValue("exhibitsViewed", () => ExhibitsViewed.Select(exhibit => exhibit.Id), ids => {
            foreach (var id in ids) {
                ExhibitsViewed.Add(Find.World.Exhibits.GetExhibitById(id));
            }
        });
    }
}