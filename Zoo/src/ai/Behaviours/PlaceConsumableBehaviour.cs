using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai; 

public class PlaceConsumableBehaviour : Behaviour {
    // Config
    private Exhibit exhibit;
    
    // State
    private IntVec2 consumablePos;

    public PlaceConsumableBehaviour() {}
    public PlaceConsumableBehaviour(Actor actor, Exhibit exhibit) : base(actor) {
        this.exhibit = exhibit;
    }

    public override void Start() {
        consumablePos = exhibit.Area.RandomTileWhere(tile => TileUtility.CanPlace(ObjectDefOf.WaterBowl, tile)); // TODO: Ensure reachable and placeable
    }

    public override IEnumerable<Step> GetSteps() {
        yield return Steps_General.GoTo(consumablePos)
            .FailOn(() => !exhibit.Exists);

        yield return Steps_General.Do(() => {
            if (exhibit.MissingWater)
                Game.RegisterEntity(GenEntity.CreateTileObject(ObjectDefOf.WaterBowl, consumablePos));
            else if (exhibit.MissingFood)
                Game.RegisterEntity(GenEntity.CreateTileObject(ObjectDefOf.Hay, consumablePos)); // TODO: Animal preferred food
        });
    }

    public override void Serialise() {
        base.Serialise();
        
        // TODO: ArchiveRef
        Find.SaveManager.ArchiveValue("exhibit", () => exhibit.Id, id => exhibit = Find.World.Exhibits.GetExhibitById(id));
        Find.SaveManager.ArchiveValue("consumablePos", ref consumablePos);
    }
}