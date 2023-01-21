using System.Numerics;
using Zoo.defs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class Guest : Actor {
    // State
    public HashSet<Exhibit> ExhibitsViewed = new ();
    
    // Properties
    public PersonComponent Person => GetComponent<PersonComponent>();
    
    public Guest(Vector2 pos, EntityDef? def) : base(pos, def) {}

    public override void Setup() {
        Person.AgeCategory = Rand.EnumValue<PersonAgeCategory>();

        if (Rand.Chance(0.05f)) {
            Person.Gender = PersonGender.NonBinary;
        } else {
            Person.Gender = Rand.Bool() ? PersonGender.Male : PersonGender.Female;
        }

        base.Setup();
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