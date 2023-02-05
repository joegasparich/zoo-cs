using System.Numerics;
using Zoo.defs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.tools;

public class Tool_Actor : Tool_Entity {
    // Properties
    public override string   Name         => "Actor Tool";
    public override ToolType Type         => ToolType.Actor;
    protected       ActorDef CurrentActor => currentEntity as ActorDef;

    public Tool_Actor(ToolManager tm) : base(tm) {}

    public override bool CanPlace(ToolGhost ghost) {
        if (!base.CanPlace(ghost)) return false;

        if (!CurrentActor.CanSwim && Find.World.Elevation.IsPositionWater(ghost.Pos.Floor())) return false;

        return true;
    }

    public override Entity CreateEntity(EntityDef def, Vector2 pos) {
        return GenEntity.CreateEntity<Actor>(def, pos);
    }
}