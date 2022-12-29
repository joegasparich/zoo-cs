using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.entities; 

public class PathFollowComponent : InputComponent {
    private const float          NodeReachedDist = 0.2f;
    private       List<IntVec2>? path;
    private       string         placeSolidHandle;
    private       bool           pathCompleted;

    public PathFollowComponent(Entity entity) : base(entity) {}

    public override void Start() {
        placeSolidHandle = Messenger.On(EventType.PlaceSolid, OnSolidPlaced);
    }

    private void OnSolidPlaced(object data) {
        if (path.NullOrEmpty()) return;

        // TODO: check performance on this, seems pretty brute force
        // TODO: Avoid allocating every time this event goes off
        var affectedTiles = (List<IntVec2>)data;

        if (path.Any(tile => affectedTiles.Contains(tile))) {
            PathTo(path.Last());
        }
    }

    public override void End() {
        Messenger.Off(EventType.PlaceSolid, placeSolidHandle);
    }

    public override void Update() {
        if (CheckPathCompleted())
            return;

        if (entity.Pos.DistanceSquared(GetCurrentNode()!.Value) < NodeReachedDist * NodeReachedDist) {
            // TODO (optimisation): should we reverse the path so we can pop?
            path!.RemoveAt(0);

            if (CheckPathCompleted())
                return;
        }

        InputVector = (GetCurrentNode()!.Value - entity.Pos).Normalised();
    }
    
    private bool CheckPathCompleted() {
        if (!path.NullOrEmpty()) return false;
        
        InputVector   = Vector2.Zero;
        pathCompleted = true;

        return true;
    }

    public void PathTo(Vector2 targetPos) {
        path = Find.World.Pathfinder.GetPath(entity.Pos.Floor(), targetPos.Floor());
        pathCompleted = false;
    }
    
    public bool ReachedDestination() {
        return pathCompleted;
    }
    
    private Vector2? GetCurrentNode() {
        if (path.NullOrEmpty()) return null; 
        return path![0];
    }

    public List<IntVec2>? GetPath() {
        return path;
    }
}