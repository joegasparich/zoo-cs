using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class PathFollowComponent : InputComponent {
    // Constants
    protected const float NodeReachedDist = 0.2f;
    
    // Config
    public AccessibilityType AccessibilityType = AccessibilityType.NoWater;
    
    // State
    private   IntVec2?                destination;
    private   List<IntVec2>?          path;
    private   Task<List<IntVec2>?>?   pathRequest;
    private   CancellationTokenSource cancelPathRequest;
    private   string                  placeSolidHandle;
    protected bool                    pathCompleted;
    
    // Properties
    public bool HasPath => path != null;

    public PathFollowComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        placeSolidHandle = Messenger.On(EventType.PlaceSolid, OnSolidPlaced);
    }

    private void OnSolidPlaced(object data) {
        if (path.NullOrEmpty()) return;

        // TODO (optimisation): check performance on this, seems pretty brute force
        // TODO (optimisation): Avoid allocating every time this event goes off
        var affectedTiles = (List<IntVec2>)data;

        if (affectedTiles.Contains(destination.Value)) {
            ResetPath();
        }
        if (path.Any(tile => affectedTiles.Contains(tile))) {
            PathTo(path.Last());
        }
    }

    public override void End() {
        Messenger.Off(EventType.PlaceSolid, placeSolidHandle);
        cancelPathRequest?.Cancel();
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

    public virtual bool PathTo(Vector2 targetPos) {
        ResetPath();

        if (entity.Pos.Floor() == targetPos.Floor()) {
            pathCompleted = true;
            return true;
        }
        if (entity.Pos.Floor().GetArea() != targetPos.Floor().GetArea()) {
            Debug.Warn($"Pather {entity.Id} tried to path outside of area");
            pathCompleted = true;
            return false;
        }

        destination                      = targetPos.Floor();
        (pathRequest, cancelPathRequest) = Find.World.Pathfinder.RequestPath(entity.Pos.Floor(), destination.Value, AccessibilityType) ?? (null, default);

        return true;
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
        cancelPathRequest?.Cancel();
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