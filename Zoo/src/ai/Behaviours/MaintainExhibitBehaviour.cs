using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai; 

public class MaintainExhibitBehaviour : Behaviour {
    // Config
    private Exhibit exhibit;
    
    // State
    private Wall exhibitEntrance;
    private IntVec2? waterBowlPos;
    private IntVec2? foodPos; // TODO: Array based on different required food types

    public MaintainExhibitBehaviour() {}
    public MaintainExhibitBehaviour(Actor actor, Exhibit exhibit) : base(actor) {
        this.exhibit = exhibit;
    }

    public override void Start() {
        exhibitEntrance = exhibit.Area.Entrances.RandomElement();

        if (exhibit.MissingWater)
            waterBowlPos = exhibit.Area.RandomTileWhere(tile => TileUtility.CanPlace(ObjectDefOf.WaterBowl, tile)); // TODO: Ensure reachable from entrancePos

        if (exhibit.MissingFood)
            foodPos = exhibit.Area.RandomTileWhere(tile => tile != waterBowlPos && TileUtility.CanPlace(ObjectDefOf.Hay, tile)); // TODO: Ensure reachable from entrancePos
    }

    public override IEnumerable<Step> GetSteps() {
        var entrancePos = exhibitEntrance.GetAdjacentTiles().First(tile => tile.GetArea() == exhibit.Area);
        var exitPos     = exhibitEntrance.GetAdjacentTiles().First(tile => tile.GetArea() != exhibit.Area);

        // Enter exhibit
        yield return Steps_General.GoTo(entrancePos)
            .FailOn(() => !exhibit.Exists);

        // Place water
        if (waterBowlPos.HasValue) {
            yield return Steps_General.GoTo(waterBowlPos.Value)
                .FailOn(() => !exhibit.Exists);

            yield return Steps_General.Do(() => {
                Game.RegisterEntity(GenEntity.CreateTileObject(ObjectDefOf.WaterBowl, waterBowlPos.Value));
                Find.Zoo.DeductFunds(ObjectDefOf.WaterBowl.Cost);
            });
        }

        // Place food
        if (foodPos.HasValue) {
            yield return Steps_General.GoTo(foodPos.Value)
                .FailOn(() => !exhibit.Exists);

            yield return Steps_General.Do(() => {
                Game.RegisterEntity(GenEntity.CreateTileObject(ObjectDefOf.Hay, foodPos.Value));
                Find.Zoo.DeductFunds(ObjectDefOf.Hay.Cost);
            });
        }

        // Exit exhibit
        yield return Steps_General.GoTo(exitPos)
            .FailOn(() => !exhibit.Exists);

        // Mark exhibit maintained
        yield return Steps_General.Do(() => exhibit.LastMaintainedTick = Game.Ticks);
    }

    public override void Serialise() {
        base.Serialise();
        
        // TODO: ArchiveRef
        Find.SaveManager.ArchiveValue("exhibit", () => exhibit.Id, id => exhibit = Find.World.Exhibits.GetExhibitById(id));
        Find.SaveManager.ArchiveValue("consumablePos", ref waterBowlPos);
        Find.SaveManager.ArchiveValue("consumablePos", ref foodPos);
        Find.SaveManager.ArchiveValue("exhibitEntrance", () => exhibitEntrance.GridPos, gridPos => exhibitEntrance = Find.World.Walls.GetWallByGridPos(gridPos));
    }
}