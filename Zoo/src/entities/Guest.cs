using System.Numerics;
using Zoo.defs;
using Zoo.world;

namespace Zoo.entities; 

public class Guest : Actor {
    private List<Exhibit> exhibitsViewed = new List<Exhibit>();
    
    public Guest(Vector2 pos, EntityDef? def) : base(pos, def) {}
}