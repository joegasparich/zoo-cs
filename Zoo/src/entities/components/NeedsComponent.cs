using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities;

public class Need {
    // Config
    public DefRef<NeedDef> Def;

    // State
    public float Value;

    public Need() {
        Value = 1;
    }
    public bool Full => Value >= 1;
}

public class NeedsComponentData : ComponentData {
    public override Type CompClass => typeof(NeedsComponent);

    public List<DefRef<NeedDef>> Needs;
}

public class NeedsComponent : Component {
    public static Type DataType => typeof(NeedsComponentData);

    // State
    public Dictionary<string, Need> Needs = new();
    
    // Properties
    public NeedsComponentData Data => (NeedsComponentData)data;

    public NeedsComponent(Entity entity, NeedsComponentData? data) : base(entity, data) {
        foreach (var need in Data.Needs) {
            Needs.Add(need.Def.Id, new Need { Def = need });
        }
    }

    public override void Update() {
        foreach (var need in Needs.Values) {
            need.Value += need.Def.Def.ChangePerTick;
        }
    }
    
    public void ModifyNeed(string defId, float amount) {
        if (!Needs.ContainsKey(defId)) {
            Debug.Warn($"Tried to modify non-existent need: {defId}");
            return;
        }
        Needs[defId].Value += amount;
        
        if (Needs[defId].Value > 1) {
            Needs[defId].Value = 1;
        }
    }

    public override InfoTab? GetInfoTab() {
        return new InfoTab("Needs", rect => {
            var listing = new Listing(rect);
            listing.Header("Needs");
            
            foreach (var need in Needs.Values) {
                listing.Label($"{need.Def.Def.Name}: {(need.Value).ToStringPercent(1)}");
            }
        });
    }

    public override void Serialise() {
        base.Serialise();
         
        Find.SaveManager.ArchiveValue("needs", ref Needs);
    }
}