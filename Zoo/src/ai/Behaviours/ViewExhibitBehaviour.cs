using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo.ai; 

public class ViewExhibitBehaviour : Behaviour {
    // Constants
    private const int ViewTicks           = 1200; // 20 seconds
    private const int MaxPathfindAttempts = 10;
    
    // Config
    private Exhibit exhibit;
    
    // State
    private IntVec2? viewingTile;
    private int      startedViewingTick  = -1;
    private int      pathfindingAttempts = 0;
    
    // Properties
    private Guest Guest => actor as Guest;

    public ViewExhibitBehaviour() {}
    public ViewExhibitBehaviour(Actor actor, Exhibit exhibit) : base(actor) {
        this.exhibit = exhibit;
    }

    public override void Start() {
        base.Start();
        
        // TODO: Add this to a can't reach cache that gets cleared every so often
        viewingTile = exhibit.ViewingTiles.RandomElement();
    }

    public override IEnumerable<Step> GetSteps() {
        if (!viewingTile.HasValue)
            yield break;

        // Go to viewing tile
        yield return Steps_General.GoTo(viewingTile.Value);
        
        // View exhibit
        yield return Steps_General.Wait(ViewTicks);
        
        // Mark exhibit viewed
        yield return Steps_General.Do(() => {
            Guest.ExhibitsViewed.Add(exhibit);
        });
    }

    public override void Serialise() {
        base.Serialise();
        
        // TODO: ArchiveReference
        Find.SaveManager.ArchiveValue("exhibit", () => exhibit.Id, id => exhibit = Find.World.Exhibits.GetExhibitById(id));
        
        Find.SaveManager.ArchiveValue("viewingTile", ref viewingTile);
        Find.SaveManager.ArchiveValue("startedViewingTick", ref startedViewingTick);
        Find.SaveManager.ArchiveValue("pathfindingAttempts", ref pathfindingAttempts);
    }
}