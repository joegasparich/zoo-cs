using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities;

public class PathFollowComponent : InputComponent {
    // Constants
    protected const float NodeReachedDist = 0.2f;
    
    // State
    private   IntVec2?                destination;
    private   List<IntVec2>?          path;
    private   Task<List<IntVec2>?>?   pathRequest;
    private   CancellationTokenSource cancelToken;
    private   string                  placeSolidHandle;
    protected bool                    pathCompleted;
    protected bool                    failedToFindPath;
    private   bool                    needNewPath;
    
    // Properties
    public         bool  HasPath            => path != null && path.Count > 0;
    public virtual bool  ReachedDestination => pathCompleted;
    public         bool  NoPathFound        => failedToFindPath;
    protected      Actor Actor              => entity as Actor;

    public PathFollowComponent(Actor actor, ComponentData? data) : base(actor, data) {}

    public override void Start() {
        base.Start();
        
        Debug.Assert(entity is Actor, "Non actor entity has path follow component");
        placeSolidHandle = Messenger.On(EventType.PlaceSolid, OnSolidPlaced);
    }

    private void OnSolidPlaced(object data) {
        if (path.NullOrEmpty()) return;

        // TODO (optimisation): check performance on this, seems pretty brute force
        // TODO (optimisation): Avoid allocating every time this event goes off
        var affectedTiles = (List<IntVec2>)data;

        // I think there's a chance here that the path could be empty while the path request is still going
        // Which could cause the actor to path through the solid
        if (path.Any(tile => affectedTiles.Contains(tile))) {
            needNewPath = true;
        }
    }

    public override void End() {
        Messenger.Off(EventType.PlaceSolid, placeSolidHandle);
        cancelToken?.Cancel();
    }

    public override void Update() {
        if (needNewPath) {
            needNewPath = false;
            PathTo(destination!.Value);
        }
        if (pathRequest != null) CheckPathRequest();
        if (path == null) return;
        if (CheckPathCompleted()) return;

        if (Actor.Pos.DistanceSquared(GetCurrentNode()!.Value) < NodeReachedDist * NodeReachedDist) {
            // TODO (optimisation): should we reverse the path so we can pop? or make it a stack
            path!.RemoveAt(0);

            if (CheckPathCompleted())
                return;
        }

        InputVector = (GetCurrentNode()!.Value - Actor.Pos).Normalised();
    }

    public override void Render() {
        if (DebugSettings.DrawPaths) {
            DrawPath();
        }
    }

    private bool CheckPathCompleted() {
        if (!path.NullOrEmpty()) return false;
        
        InputVector   = Vector2.Zero;
        pathCompleted = true;
        destination   = null;

        return true;
    }

    public virtual bool PathTo(Vector2 targetPos) {
        ResetPath();

        if (Actor.Pos.Floor() == targetPos.Floor()) {
            pathCompleted = true;
            return true;
        }
        if (Actor.Pos.Floor().GetArea() != targetPos.Floor().GetArea()) {
            Debug.Warn($"Pather {Actor.Id} tried to path outside of area");
            failedToFindPath = true;
            path             = null;
            return false;
        }

        var accesibility = Actor.Accessibility;

        // Get back onto footpath
        if (accesibility == AccessibilityType.PathsOnly && !Actor.Pos.Floor().GetFootPath().Exists)
            accesibility = AccessibilityType.NoWater;

        destination                = targetPos.Floor();
        (pathRequest, cancelToken) = Find.World.Pathfinder.RequestPath(Actor.Pos.Floor(), destination.Value, accesibility) ?? (null, default);

        return true;
    }

    private void CheckPathRequest() {
        if (pathRequest is not { IsCompleted: true }) return;
        if (pathRequest.IsCanceled) {
            pathRequest = null;
            return;
        }

        path        = pathRequest.Result;
        pathRequest = null;
        
        if (path.NullOrEmpty()) {
            failedToFindPath = true;
            path             = null;
            return;
        }
        path!.Dequeue(); // Dequeue first node as it's the current position
        pathCompleted = false;
    }

    public void ResetPath() {
        InputVector      = Vector2.Zero;
        path             = null;
        pathCompleted    = false;
        failedToFindPath = false;
        destination      = null;
        if (cancelToken != null)
            Find.World.Pathfinder.CancelPathRequest(cancelToken);
        pathRequest = null;
        cancelToken = null;
    }
    
    private Vector2? GetCurrentNode() {
        if (path.NullOrEmpty()) return null; 
        
        return path![0] + new Vector2(0.5f, 0.5f);
    }

    public List<IntVec2>? GetPath() {
        return path;
    }

    public void DrawPath() {
        if (!path.NullOrEmpty()) {
            Debug.DrawLine(
                entity.Pos,
                path[0] + new Vector2(0.5f, 0.5f),
                Color.RED,
                true
            );

            for (var i = 1; i < path.Count; i++) {
                Debug.DrawLine(
                    path[i - 1] + new Vector2(0.5f, 0.5f),
                    path[i]     + new Vector2(0.5f, 0.5f),
                    Color.RED,
                    true
                );
            }
        }
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("destination", ref destination);

        if (Find.SaveManager.Mode == SerialiseMode.Loading && destination.HasValue)
            PathTo(destination.Value);
    }
}