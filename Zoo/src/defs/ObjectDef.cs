using System.Numerics;

namespace Zoo.defs; 

public enum ObjectType {
    Foliage,
    Building,
    Consumable,
    Misc
}

public class ObjectDef : EntityDef {
    public ObjectType Type;
    public Vector2    Size             = Vector2.One;
    public bool       Solid            = true;
    public int        Cost             = 1;
    public bool       CanPlaceOnSlopes = false;
    public bool       CanPlaceInWater  = false;
    public bool       CanRotate        = false;
    public bool       NeedsBlueprint   = false;

    public ObjectDef() {
        IsStatic = true;
    }
}