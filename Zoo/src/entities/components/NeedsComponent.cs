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

public class NeedsComponentData : ComponentData {
    public override Type CompClass => typeof(NeedsComponent);

    public List<DefRef<NeedDef>> Needs;
}

public class NeedsComponent : Component {
    // State
    public List<Need>         Needs = new();
    
    // Properties
    public NeedsComponentData Data => (NeedsComponentData)data;
    
    public NeedsComponent(Entity entity, NeedsComponentData? data) : base(entity, data) {}

    public override void Start()
    {
        base.Start();
        
        foreach (var need in Data.Needs) {
            Needs.Add(new Need { Def = need });
        }
    }

    public override void Update() {
        foreach (var need in Needs) {
            if (need.Value > Need.MaxNeed) {
                need.Value = Need.MaxNeed;
            }
            
            need.Value += need.Def.ChangePerTick;
        }
    }
}