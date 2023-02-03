using Zoo.defs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.ai; 

public class ConsumeBehaviour : Behaviour {
    // Constants
    private int EatTicks = 300; // 5 seconds
    
    // Config
    private NeedDef need;
    
    // State
    private Entity? consumable;
    
    // Properties
    public  NeedsComponent       Needs      => actor.GetComponent<NeedsComponent>();
    private bool                 CanConsume => consumable != null && !consumable.Despawned && actor.Pos.InDistOf(consumable.Pos, 1);
    private ConsumableComponent? Consumable => consumable != null ? consumable.GetComponent<ConsumableComponent>() : null;
    
    public ConsumeBehaviour() {}
    public ConsumeBehaviour(Actor actor, NeedDef need) : base(actor) {
        Debug.Assert(Needs != null, "Consume behaviour requires the actor to have a needs component");
        
        this.need = need;
    }

    public override void Start() {
        base.Start();
        
        consumable = GetClosestConsumableOfType(need.Id);
    }

    public override IEnumerable<Step> GetSteps() {
        // Go to consumable
        yield return Steps_General.GoTo(consumable.Pos)
            .FailOn(() => consumable.Despawned);

        // Eat for a while
        yield return Steps_General.Wait(EatTicks)
            .FailOn(() => consumable.Despawned);

        // Modify need
        yield return Steps_General.Do(() => {
            var amount = Consumable.Consume(1 - Needs.Needs[Consumable.Data.Need.Def.Id].Value);
            Needs.ModifyNeed(Consumable.Data.Need.Def.Id, amount);
        });
    }

    private Entity? GetClosestConsumableOfType(string needDefId) {
        Entity? closest            = null;
        var     closestDistSquared = float.PositiveInfinity;
        
        foreach (var entity in actor.Area.GetContainedEntities(EntityTag.Consumable)) {
            var consumable = entity.GetComponent<ConsumableComponent>();
            Debug.Assert(consumable != null, "Entity with consumable tag must have consumable component");
            if (consumable.Data.Need.Def.Id != needDefId) continue;

            var distSquared = entity.Pos.DistanceSquared(actor.Pos); 
            if (distSquared < closestDistSquared) {
                closest            = entity;
                closestDistSquared = distSquared;
            }
        }

        return closest;
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveDef("need", ref need);
        Find.SaveManager.ArchiveValue("consumable", () => consumable.Id, id => consumable = Game.GetEntityById(id));
    }
}