using Zoo.defs;

namespace Zoo.entities;

public class Need {
    // Constants
    public const float MaxNeed = 100;
    
    // Config
    public NeedDef Def;

    // State
    public float Value;

    public Need() {
        Value = MaxNeed;
    }
}

public class NeedsComponent : Component {
    // State
    public List<Need> Needs = new();
    
    public NeedsComponent(Entity entity) : base(entity) {}

    public override void Update() {
        foreach (var need in Needs) {
            if (need.Value > Need.MaxNeed) {
                need.Value = Need.MaxNeed;
            }
            
            need.Value += need.Def.ChangePerTick;
        }
    }
}