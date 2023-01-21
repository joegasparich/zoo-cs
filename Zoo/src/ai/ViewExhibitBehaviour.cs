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
    
    public override void Update() {
        base.Update();
        
        if (pathfindingAttempts >= MaxPathfindAttempts) {
            completed = true;
            // TODO: Add this to a can't reach cache that gets cleared every so often
            return;
        }
        
        // Viewing exhibit 
        if (startedViewingTick > 0) {
            if (startedViewingTick + ViewTicks < Game.Ticks) {
                Guest.ExhibitsViewed.Add(exhibit);
                completed = true;
            }
            
            return;
        }

        // Reached view tile
        if (Pather.ReachedDestination) {
            startedViewingTick = Game.Ticks;
            return;
        }

        // Viewing tile could not be pathed to
        if (Pather.NoPathFound)
            viewingTile = null;

        // Go to view tile
        if (!viewingTile.HasValue) {
            viewingTile = exhibit.ViewingTiles.RandomElement();
            Pather.PathTo(viewingTile.Value);
            pathfindingAttempts++;
        }
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("viewingTile", ref viewingTile);
        Find.SaveManager.ArchiveValue("startedViewingTick", ref startedViewingTick);
        Find.SaveManager.ArchiveValue("pathfindingAttempts", ref pathfindingAttempts);
    }
}