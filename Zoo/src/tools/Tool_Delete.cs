using System.Numerics;
using Newtonsoft.Json.Linq;
using Raylib_cs;
using Zoo.world;
using Zoo.entities;
using Zoo.util;

namespace Zoo.tools; 

public class Tool_Delete : Tool {
    // Constants
    private static readonly Color GhostColour = new (255, 102, 26, 255);
    private const string TileObjectsSaveKey = "tileObjects";
    private const string WallsSaveKey = "walls";
    private const string PathsSaveKey = "paths";
    
    // Virtual Properties
    public override string   Name => "Delete Tool";
    public override ToolType Type => ToolType.Delete;
    
    // State
    private bool    isDragging;
    private IntVec2 dragStartTile;

    public Tool_Delete(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type        = GhostType.Square;
        Ghost.Snap        = true;
        Ghost.Elevate     = true;
        Ghost.GhostColour = GhostColour;
        Ghost.Visible     = true;
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            dragStartTile = evt.mouseWorldPos.Floor();
            isDragging    = true;
            evt.Consume();
        }
        
        if (isDragging && evt.mouseUp == MouseButton.MOUSE_BUTTON_LEFT) {
            var tileObjects = GetHighlightedTileObjects();
            var walls       = GetHighlightedWalls();
            var paths       = GetHighlightedFootPaths();

            var undoData = GetUndoData(tileObjects, walls, paths);

            // Delete tile objects
            foreach(var t in tileObjects) {
                t.Destroy();
            }
            // Delete paths
            foreach(var p in paths) {
                Find.World.FootPaths.RemovePathAtTile(p.Pos);
            }
            // Delete walls
            foreach(var w in walls) {
                Find.World.Walls.RemoveWall(w);
            }
            
            toolManager.PushAction(new ToolAction() {
                Name = "Delete",
                Data = undoData,
                Undo = Undo
            });

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

    private HashSet<Entity> highlightedTileObjects = new ();
    private HashSet<Entity> GetHighlightedTileObjects() {
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
    
    private HashSet<FootPath> highlightedFootPaths = new ();
    private HashSet<FootPath> GetHighlightedFootPaths() {
        highlightedFootPaths.Clear();
        
        var dragRect = GetDragRect();
        
        for (var i = dragRect.x; i < dragRect.XMax(); i++) {
            for (var j = dragRect.y; j < dragRect.YMax(); j++) {
                var tile = new IntVec2(i.FloorToInt(), j.FloorToInt());
                if (!Find.World.IsPositionInMap(tile)) continue;

                var footpath = Find.World.FootPaths.GetFootPathAtTile(tile);
                if (footpath is not { Exists: true }) continue;
                highlightedFootPaths.Add(footpath);
            }
        }

        return highlightedFootPaths;
    }
    
    private HashSet<Wall> highlightedWalls = new ();
    private HashSet<Wall> GetHighlightedWalls() {
        highlightedWalls.Clear();
        
        var dragRect = GetDragRect();
        
        for (var i = dragRect.x; i < dragRect.XMax(); i++) {
            for (var j = dragRect.y; j < dragRect.YMax(); j++) {
                var tile = new IntVec2(i.FloorToInt(), j.FloorToInt());
                if (!Find.World.IsPositionInMap(tile)) continue;

                foreach(var wall in Find.World.Walls.GetWallsSurroundingTile(tile)) {
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
    
    private JObject GetUndoData(IEnumerable<Entity> tileObjects, IEnumerable<Wall> walls, IEnumerable<FootPath> paths) {
        var undoData = new JObject();
            
        // Tile objects
        var tileObjectData = new JArray();
        foreach (var tileObject in tileObjects) {
            tileObjectData.Add(Find.SaveManager.Serialise(tileObject));
        }
        undoData.Add(TileObjectsSaveKey, tileObjectData);
            
        // Walls
        var wallData = new JArray();
        foreach (var wall in walls) {
            wallData.Add(Find.SaveManager.Serialise(wall));
        }
        undoData.Add(WallsSaveKey, wallData);
           
        // Paths
        var pathData = new JArray();
        foreach (var path in paths) {
            pathData.Add(Find.SaveManager.Serialise(path));
        }
        undoData.Add(PathsSaveKey, pathData);

        return undoData;
    }
    
    private void Undo(object json) {
        var undoData = (JObject)json;
        
        // Tile objects
        var tileObjects = undoData[TileObjectsSaveKey] as JArray;
        EntitySerialiseUtility.LoadEntities(tileObjects);
        
        // Walls
        var walls = undoData[WallsSaveKey] as JArray;
        var i = 0;
        foreach (var value in walls.Select(wallData => Find.World.Walls.GetWallByGridPos(wallData["gridPos"].Value<IntVec2>()))) {
            Find.SaveManager.CurrentSaveNode = walls[i++] as JObject; 
            value.Serialise();
        }
        
        // Paths
        var paths = undoData[PathsSaveKey] as JArray;
        i = 0;
        foreach (var value in paths.Select(pathData => Find.World.FootPaths.GetFootPathAtTile(pathData["pos"].Value<IntVec2>()))) {
            Find.SaveManager.CurrentSaveNode = paths[i++] as JObject; 
            value.Serialise();
        }
    }
}