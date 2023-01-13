using System.Numerics;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class AreaPathFollowComponent : PathFollowComponent {
    // State
    private Vector2?    destination;
    private List<Area>? areaPath;
    private Area?       currentArea;
    private Wall?       currentDoor;
    private Vector2?    deferredTargetPos; // This is for when we receive a new path when moving through a door
    private Vector2?    enterDoorPos;
    private bool        areaPathCompleted;
    
    public AreaPathFollowComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Update() {
        if (areaPath.NullOrEmpty() && !enterDoorPos.HasValue) {
            base.Update();
            currentDoor = null;
            return;
        }

        if (!areaPath.NullOrEmpty() && currentDoor == null) {
            // We don't have a door to go to
            // Find and path to the next dor
            var nextArea = areaPath!.First();
            var minDistSquared = float.MaxValue;
            foreach (var door in currentArea.ConnectedAreas[nextArea]) {
                var distSquared = entity.Pos.DistanceSquared(door.WorldPos);
                if (distSquared < minDistSquared) {
                    currentDoor    = door;
                    minDistSquared = distSquared;
                }
            }
            var doorTiles  = currentDoor.GetAdjacentTiles();
            var targetTile = doorTiles.First(tile => Find.World.Areas.GetAreaAtTile(tile) == currentArea);
            base.PathTo(targetTile);
        }

        if (currentDoor != null & !enterDoorPos.HasValue) {
            // We have a door to go through and aren't currently going through it
            // Head to door
            base.Update();

            if (base.ReachedDestination()) {
                // We've made it to the door
                var doorTiles = currentDoor.GetAdjacentTiles();
                enterDoorPos = doorTiles.First(tile => Find.World.Areas.GetAreaAtTile(tile) != currentArea) + new Vector2(0.5f, 0.5f);
            }
        }

        if (enterDoorPos.HasValue) {
            // We are going through the door
            InputVector = (enterDoorPos!.Value - entity.Pos).Normalised();

            if (entity.Pos.DistanceSquared(enterDoorPos.Value) < NodeReachedDist * NodeReachedDist) {
                // We've made it through the door
                enterDoorPos = null;
                currentDoor  = null;

                if (deferredTargetPos.HasValue) {
                    PathTo(deferredTargetPos.Value);
                    deferredTargetPos = null;
                    return;
                }

                currentArea = areaPath.Dequeue();

                if (!areaPath.Any()) {
                    // We're in the final area, path to final destination
                    base.PathTo(destination.Value);
                    areaPathCompleted = pathCompleted;
                }
            }
        }
    }

    public override bool PathTo(Vector2 target) {
        if (enterDoorPos.HasValue) {
            deferredTargetPos = target;
            return true;
        }

        ResetAreaPath();

        var curArea = Find.World.Areas.GetAreaAtTile(entity.Pos.Floor());
        var targetArea = Find.World.Areas.GetAreaAtTile(target.Floor());

        Debug.Assert(curArea != null);
        Debug.Assert(targetArea != null);

        if (curArea != targetArea) {
            areaPath = Find.World.Areas.BFS(curArea, targetArea);
            if (areaPath.Any()) {
                currentArea = areaPath.Dequeue();
                destination = target;
                return true;
            }
            return false;
        }
        
        return base.PathTo(target);
    }

    public override bool ReachedDestination() {
        return areaPathCompleted;
    }

    private void ResetAreaPath() {
        areaPath          = null;
        currentArea       = null;
        currentDoor       = null;
        destination       = null;
        enterDoorPos      = null;
        deferredTargetPos = null;
    }

    public override void Serialise() {
        base.Serialise();

        Find.SaveManager.ArchiveValue("areaDestination", ref destination);
        Find.SaveManager.ArchiveValue("enterDoorPos", ref enterDoorPos);

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            if (enterDoorPos.HasValue) {
                deferredTargetPos = destination;
            } else if (destination.HasValue)
                PathTo(destination.Value);
        }
    }
}