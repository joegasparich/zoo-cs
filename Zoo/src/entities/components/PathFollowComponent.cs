using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.entities; 

public class PathFollowComponent : InputComponent {
    protected const float                 NodeReachedDist = 0.2f;
    private         Vector2?              destination;
    private         List<IntVec2>?        path;
    private         Task<List<IntVec2>?>? pathRequest;
    private         string                placeSolidHandle;
    protected       bool                  pathCompleted;

    public PathFollowComponent(Entity entity) : base(entity) {}

    public override void Start() {
        placeSolidHandle = Messenger.On(EventType.PlaceSolid, OnSolidPlaced);
    }

    private void OnSolidPlaced(object data) {
        if (path.NullOrEmpty()) return;

        // TODO (optimisation): check performance on this, seems pretty brute force
        // TODO (optimisation): Avoid allocating every time this event goes off
        var affectedTiles = (List<IntVec2>)data;

        if (path.Any(tile => affectedTiles.Contains(tile))) {
            PathTo(path.Last());
        }
    }

    public override void End() {
        Messenger.Off(EventType.PlaceSolid, placeSolidHandle);
    }

    public override void Update() {
        if (pathRequest != null) CheckPathRequest();
        if (path == null) return;
        if (CheckPathCompleted()) return;

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
        
        InputVector    = Vector2.Zero;
        pathCompleted  = true;
        destination = null;

        return true;
    }

    public virtual void PathTo(Vector2 targetPos) {
        ResetPath();

        if (entity.Pos.Floor() == targetPos.Floor()) {
            pathCompleted = true;
            return;
        }

        destination = targetPos;
        pathRequest    = Find.World.Pathfinder.RequestPath(entity.Pos.Floor(), targetPos.Floor());
    }

    private void CheckPathRequest() {
        if (pathRequest is not { IsCompleted: true }) return;

        path        = pathRequest.Result;
        pathRequest = null;
        
        if (path.NullOrEmpty()) {
            path = null;
            return;
        }
        path!.Dequeue(); // Dequeue first node as it's the current position
        pathCompleted = false;
    }

    private void ResetPath() {
        InputVector    = Vector2.Zero;
        path           = null;
        pathCompleted  = false;
        destination = null;
    }
    
    public virtual bool ReachedDestination() {
        return pathCompleted;
    }
    
    private Vector2? GetCurrentNode() {
        if (path.NullOrEmpty()) return null; 
        
        return path![0] + new Vector2(0.5f, 0.5f);
    }

    public List<IntVec2>? GetPath() {
        return path;
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("destination", ref destination);
        
        if (Find.SaveManager.Mode == SerialiseMode.Loading && destination.HasValue)
            PathTo(destination.Value);
    }
}