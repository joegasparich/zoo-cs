using System.Numerics;
using Zoo.defs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class Actor : Entity {
    // Constants
    private const int AreaAccessibilityCacheInterval = 600; // 10 seconds
    
    // Cache
    public HashSet<Area> AccessibleAreas = new();
    public HashSet<Exhibit> AccesibleExhibits = new ();
    
    // Components
    protected RenderComponent Renderer => GetComponent<RenderComponent>()!;
    public PathFollowComponent Pather => GetComponent<PathFollowComponent>()!;
    
    // Properties
    public override ActorDef          Def           => (ActorDef)base.Def;
    public          AccessibilityType Accessibility => Def.Accessibility;
    public          bool              CanUseDoors   => HasComponent<AreaPathFollowComponent>();
    public          Area              Area          => Find.World.Areas.GetAreaAtTile(Pos.Floor());
    public          Exhibit?          Exhibit       => Find.World.Exhibits.GetExhibitByArea(Area);

    public Actor(Vector2 pos, ActorDef? def) : base(pos, def) {}

    public Need? GetNeed(NeedDef def) => GetComponent<NeedsComponent>()?.Needs[def.Id];

    public override void Update() {
        base.Update();
        
        if (Game.Ticks % AreaAccessibilityCacheInterval == 0) {
            UpdateAccesibleAreasCache();
        }
    }

    private void UpdateAccesibleAreasCache() {
        AccesibleExhibits.Clear();
        foreach (var area in Find.World.Areas.GetAreas()) {
            if (area.ReachableFrom(Area)) {
                AccessibleAreas.Add(area);
            }
        }
        foreach (var exhibit in Find.World.Exhibits.Exhibits) {
            if (AccessibleAreas.Contains(exhibit.Area))
                AccesibleExhibits.Add(exhibit);
        }
    }
}