using System.Numerics;
using Zoo.ai;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities; 

public class Animal : Actor {
    // Properties
    public override AnimalDef Def => (AnimalDef)base.Def;
    public bool IsAsleep => GetComponent<AnimalBehaviourComponent>().CurrentBehaviour is SleepBehaviour;

    public Animal(Vector2 pos, AnimalDef def) : base(pos, def) {}

    public override void Setup() {
        base.Setup();
        
        Messenger.Fire(EventType.AnimalPlaced, this);
    }

    public override void Destroy() {
        base.Destroy();
        
        Messenger.Fire(EventType.AnimalRemoved, this);
    }

    public override List<InfoTab> GetInfoTabs() {
        var tabs = new List<InfoTab>();
        tabs.Add(new InfoTab("General", rect => {
            var listing = new Listing(rect);
            listing.Header(Def.Name.Capitalise());
            listing.Label($"Exhibit: {Exhibit.Name}");
            listing.Label($"Can swim: {Def.CanSwim.ToString().Capitalise()}");
        }));
        tabs.AddRange(base.GetInfoTabs());
        return tabs;
    }
}