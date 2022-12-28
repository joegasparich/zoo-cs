using System.Numerics;
using Raylib_cs;
using Zoo.world;
using Zoo.entities;
using Zoo.util;

namespace Zoo.tools; 

public class Tool_Delete : Tool {
    private static readonly Color GhostColour = new (255, 102, 26, 255);
    
    private bool    isDragging;
    private IntVec2 dragStartTile;
    
    public override string   Name => "Delete Tool";
    public override ToolType Type => ToolType.Delete;

    public Tool_Delete(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type        = GhostType.Square;
        Ghost.Snap        = true;
        Ghost.Elevate     = true;
        Ghost.GhostColour = GhostColour;
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            dragStartTile = evt.mouseWorldPos.Floor();
            isDragging    = true;
            evt.Consume();
        }
        
        if (isDragging && evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            var tileObjects = GetHighlightedTileObjects();
            
            // Delete tile objects
            foreach(var t in tileObjects) {
                t.Destroy();
            }
            // Delete paths
            foreach(var p in GetHighlightedFootPaths()) {
                Find.World.FootPaths.RemovePathAtTile(p.Pos);
            }
            // Delete walls
            foreach(var w in GetHighlightedWalls()) {
                Find.World.Walls.RemoveWall(w);
            }

            Ghost.Follow = true;
            Ghost.Scale  = IntVec2.One;
            
            isDragging = false;
            evt.Consume();
        }
    }

    public override void Update() {
        if (isDragging) {
            var dragRect = GetDragRect();
            
            Ghost.Follow = false;
            Ghost.Pos    = dragRect.Position();
            Ghost.Scale  = dragRect.Dimensions();

            foreach(var t in GetHighlightedTileObjects()) {
                t.GetComponent<RenderComponent>()!.OverrideColour = GhostColour;
            }
            foreach(var p in GetHighlightedFootPaths()) {
                p.OverrideColour = GhostColour;
            }
            foreach(var w in GetHighlightedWalls()) {
                w.OverrideColour = GhostColour;
            } 
        }
    }

    private Rectangle GetDragRect() {
        var dragEndTile = Find.Input.GetMouseWorldPos().Floor();
        var upperLeft = new IntVec2(Math.Min(dragStartTile.X, dragEndTile.X), Math.Min(dragStartTile.Y, dragEndTile.Y));
        var bottomRight = new IntVec2(Math.Max(dragStartTile.X, dragEndTile.X), Math.Max(dragStartTile.Y, dragEndTile.Y));
        var width = bottomRight.X - upperLeft.X + 1;
        var height = bottomRight.Y - upperLeft.Y + 1;
        
        return new Rectangle(upperLeft.X, upperLeft.Y, width, height);
    }

    private List<Entity> highlightedTileObjects = new ();
    private List<Entity> GetHighlightedTileObjects() {
        highlightedTileObjects.Clear();
        
        var dragRect = GetDragRect();

        for (var i = dragRect.x; i < dragRect.XMax(); i++) {
            for (var j = dragRect.y; j < dragRect.YMax(); j++) {
                var tile = new IntVec2(i.FloorToInt(), j.FloorToInt());
                if (!Find.World.IsPositionInMap(tile)) continue;

                var tileObject = Find.World.GetTileObjectAtTile(tile);
                if (tileObject == null) continue;
                highlightedTileObjects.Add(tileObject);
            }
        }

        return highlightedTileObjects;
    }
    
    private List<FootPath> highlightedFootPaths = new ();
    private List<FootPath> GetHighlightedFootPaths() {
        highlightedFootPaths.Clear();
        
        var dragRect = GetDragRect();
        
        for (var i = dragRect.x; i < dragRect.XMax(); i++) {
            for (var j = dragRect.y; j < dragRect.YMax(); j++) {
                var tile = new IntVec2(i.FloorToInt(), j.FloorToInt());
                if (!Find.World.IsPositionInMap(tile)) continue;

                var footpath = Find.World.FootPaths.GetPathAtTile(tile);
                if (footpath is not { Exists: true }) continue;
                highlightedFootPaths.Add(footpath);
            }
        }

        return highlightedFootPaths;
    }
    
    private List<Wall> highlightedWalls = new ();
    private List<Wall> GetHighlightedWalls() {
        highlightedWalls.Clear();
        
        var dragRect = GetDragRect();
        
        for (var i = dragRect.x; i < dragRect.XMax(); i++) {
            for (var j = dragRect.y; j < dragRect.YMax(); j++) {
                var tile = new IntVec2(i.FloorToInt(), j.FloorToInt());
                if (!Find.World.IsPositionInMap(tile)) continue;

                foreach(var wall in Find.World.Walls.GetSurroundingWalls(tile)) {
                    if (!wall.Exists) continue;

                    var opposite = Find.World.Walls.GetOppositeTile(wall, tile);
                    if (opposite.HasValue && dragRect.Contains(opposite.Value + new Vector2(0.5f, 0.5f))) {
                        highlightedWalls.Add(wall);
                    }
                }
            }
        }

        return highlightedWalls;
    }
}