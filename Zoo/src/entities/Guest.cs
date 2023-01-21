using System.Numerics;
using Zoo.defs;

namespace Zoo.entities; 

public class Guest : Actor {
    public Guest(Vector2 pos, EntityDef? def) : base(pos, def) {}
}