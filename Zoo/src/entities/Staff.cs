using System.Numerics;
using Zoo.defs;
using Zoo.world;

namespace Zoo.entities; 

public class Staff : Actor {
    // Properties
    public PersonComponent Person => GetComponent<PersonComponent>();
    
    public Staff(Vector2 pos, ActorDef? def) : base(pos, def) {}
    
    public override void Setup() {
        base.Setup();
        
        Find.Zoo.Staff.Add(this);
    }

    public override void Destroy() {
        Find.Zoo.Staff.Remove(this);
        
        base.Destroy();
    }
}