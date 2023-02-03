using System.Numerics;
using Zoo.defs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class Actor : Entity {
    // Components
    protected RenderComponent Renderer => GetComponent<RenderComponent>()!;
    public PathFollowComponent Pather => GetComponent<PathFollowComponent>()!;
    
    // Properties
    public override ActorDef          Def           => (ActorDef)base.Def;
    public          AccessibilityType Accessibility => Def.Accessibility;
    public          Area              Area          => Find.World.Areas.GetAreaAtTile(Pos.Floor());
    public          Exhibit?          Exhibit       => Find.World.Exhibits.GetExhibitByArea(Area);

    public Actor(Vector2 pos, ActorDef? def) : base(pos, def) {}

    public Need? GetNeed(NeedDef def) => GetComponent<NeedsComponent>()?.Needs[def.Id];

}