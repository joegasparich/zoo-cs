using System.Numerics;
using Zoo.defs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public abstract class Actor : Entity {
    // Components
    protected RenderComponent Renderer => GetComponent<RenderComponent>()!;
    
    // Properties
    public override ActorDef          Def           => (ActorDef)base.Def;
    public          AccessibilityType Accessibility => Def.RespectsPaths ? AccessibilityType.PathsOnly : Def.CanSwim ? AccessibilityType.NoSolidIgnorePaths : AccessibilityType.NoWaterIgnorePaths;
    public          Area              Area          => Find.World.Areas.GetAreaAtTile(Pos.Floor());
    public          Exhibit           Exhibit       => Find.World.Exhibits.GetExhibitByArea(Area);

    protected Actor(Vector2 pos, EntityDef? def) : base(pos, def) {}

    public Need? GetNeed(NeedDef def) => GetComponent<NeedsComponent>()?.Needs[def.Id];

}