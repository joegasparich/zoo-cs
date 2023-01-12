using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities;

public class Need {
    // Constants
    public const float MaxNeed = 100;
    
    // Config
    public DefRef<NeedDef> Def;

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

    public NeedsComponent(Entity entity, NeedsComponentData? data) : base(entity, data) {
        foreach (var need in Data.Needs) {
            Needs.Add(new Need { Def = need });
        }
    }

    public override void Update() {
        foreach (var need in Needs) {
            if (need.Value > Need.MaxNeed) {
                need.Value = Need.MaxNeed;
            }
            
            need.Value += need.Def.Def.ChangePerTick;
        }
    }

    public override InfoTab? GetInfoTab() {
        return new InfoTab("Needs", rect => {
            var listing = new Listing(rect);
            listing.Header("Needs");
            
            foreach (var need in Needs) {
                listing.Label($"{need.Def.Def.Name}: {(need.Value / Need.MaxNeed).ToStringPercent(1)}");
            }
        });
    }

    public override void Serialise() {
        base.Serialise();
         
        Find.SaveManager.ArchiveValue("needs", ref Needs);
    }
}