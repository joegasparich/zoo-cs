using System.Numerics;
using Zoo.ai;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities; 

public class Animal : Actor {
    // Cache
    public bool MissingFoodSource = false;
    public bool MissingWaterSource = false;
    
    // Properties
    public override AnimalDef Def => (AnimalDef)base.Def;
    public bool IsAsleep => GetComponent<BehaviourComponent>().CurrentBehaviour is SleepBehaviour;

    public Animal(Vector2 pos, AnimalDef def) : base(pos, def) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);

        Find.Zoo.Animals.Add(this);
        Messenger.Fire(EventType.AnimalPlaced, this);
    }

    public override void Destroy() {
        Messenger.Fire(EventType.AnimalRemoved, this);
        Find.Zoo.Animals.Remove(this);
        
        base.Destroy();
    }

    public override List<InfoTab> GetInfoTabs() {
        var tabs = new List<InfoTab>();
        tabs.Add(new InfoTab("General", rect => {
            var listing = new Listing(rect);
            listing.Header(Def.Name.Capitalise());
            if (Exhibit != null) listing.Label($"Exhibit: {Exhibit.Name}");
            listing.Label($"Can swim: {Def.CanSwim.ToString().Capitalise()}");
        }));
        tabs.AddRange(base.GetInfoTabs());
        return tabs;
    }
}