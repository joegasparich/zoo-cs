using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;

namespace Zoo.world;

public enum Side {
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public enum Direction {
    N, NE, E, SE, S, SW, W, NW
};

public enum AccessibilityType {
    NoSolid,
    NoWater,
    NoSolidIgnorePaths,
    NoWaterIgnorePaths,
    PathsOnly,
}

public enum TileCost {
    None = 0,
    Preferred = 1,
    Normal = 30,
}

public class World : ISerialisable {
    // Constants
    public const int WorldScale      = 16;
    public const int LargerThanWorld = 10000;

    // Config
    public int Width;
    public int Height;

    // Collections
    private readonly Dictionary<string, List<Entity>>       entitiesByTileDynamic = new(); // Gets cleared every frame
    private readonly Dictionary<string, List<Entity>>       entitiesByTileStatic  = new(); // Must be manually added to and removed from
    private readonly Dictionary<string, TileObject>         tileObjectMap         = new();
    private readonly Dictionary<AccessibilityType, int[][]> accessibilityGrids    = new();

    // Grids
    public ElevationGrid  Elevation  { get; }
    public TerrainGrid    Terrain    { get; }
    public WallGrid       Walls      { get; }
    public FootPathGrid   FootPaths  { get; }
    public AreaManager    Areas      { get; }
    public ExhibitManager Exhibits   { get; }
    public Pathfinder     Pathfinder { get; private set; }

    private bool isSetup = false;

    public World(int width, int height) {
        Width  = width;
        Height = height;

        Elevation = new ElevationGrid(Width + 1, Height + 1);
        Terrain   = new TerrainGrid(Width * TerrainGrid.TerrainScale, Height * TerrainGrid.TerrainScale);
        Walls     = new WallGrid(Width, Height);
        FootPaths = new FootPathGrid(Width, Height);
        Areas     = new AreaManager();
        Exhibits  = new ExhibitManager();
    }

    public void Setup() {
        if (isSetup) return;

        Elevation.Setup();
        Terrain.Setup();
        Walls.Setup();
        FootPaths.Setup();
        Areas.Setup(Find.Zoo.Entrance);
        Exhibits.Setup();
        Pathfinder = new Pathfinder(Width, Height);

        PopulateAccessibilityGrids();

        isSetup = true;

        Debug.Log("World setup complete");
    }
    
    public void Reset() {
        Elevation.Reset();
        Terrain.Reset();
        Walls.Reset();
        FootPaths.Reset();
        Areas.Reset();
        Exhibits.Reset();
        Pathfinder.Reset();

        entitiesByTileDynamic.Clear();
        entitiesByTileStatic.Clear();
        accessibilityGrids.Clear();
        
        isSetup = false;
    }

    // Note: This must be called before entity PreUpdate
    public void PreUpdate() {
        entitiesByTileDynamic.Clear();
    }
    public void Update() {}

    public void PostUpdate() {}

    public void Render() {
        Terrain.Render();
        Elevation.Render();
        Walls.Render();
        FootPaths.Render();
    }
    public void RenderDebug() {
        if (DebugSettings.CellGrid) RenderDebugCellGrid();
        if (DebugSettings.TerrainChunks) Terrain.RenderChunkDebug();
        if (DebugSettings.ElevationGrid) Elevation.RenderDebug();
        if (DebugSettings.AreaGrid) Areas.RenderDebugAreaGrid();
        if (DebugSettings.PathfindingGrid) Pathfinder.DrawDebugGrid();
    }

    public void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            var area = Areas.GetAreaAtTile(evt.mouseWorldPos.Floor());
            if (area == null) return;
            var exhibit = Exhibits.GetExhibitByArea(area);
            if (exhibit == null) return;
            Find.UI.PushWindow(new Dialog_ExhibitInfo(exhibit));
        }
    }
    
    public void RegisterTileObject(TileObject tileObject) {
        foreach (var tile in tileObject.GetOccupiedTiles()) {
            tileObjectMap.Add(tile.ToString(), tileObject);
        }
    }
    
    public void UnregisterTileObject(TileObject tileObject) {
        foreach (var tile in tileObject.GetOccupiedTiles()) {
            tileObjectMap.Remove(tile.ToString());
        }
    }
    
    // TODO: Remove allocations from these functions
    public void OccupyTileStatic(Entity entity) {
        foreach (var tile in entity.GetOccupiedTiles()) {
            if (!entitiesByTileStatic.ContainsKey(tile.ToString()))
                entitiesByTileStatic.Add(tile.ToString(), new List<Entity>());

            entitiesByTileStatic[tile.ToString()].Add(entity);
        }
    }

    public void UnoccupyTileStatic(Entity entity) {
        foreach (var tile in entity.GetOccupiedTiles()) {
            entitiesByTileStatic[tile.ToString()].Remove(entity);
        }
    }
    
    public void OccupyTileDynamic(Entity entity) {
        foreach (var tile in entity.GetOccupiedTiles()) {
            if (!entitiesByTileDynamic.ContainsKey(tile.ToString()))
                entitiesByTileDynamic.Add(tile.ToString(), new List<Entity>());

            entitiesByTileDynamic[tile.ToString()].Add(entity);
        }
    }

    private void PopulateAccessibilityGrids() {
        accessibilityGrids.Clear();
        
        foreach(var type in Enum.GetValues(typeof(AccessibilityType))) {
            var grid = new int[Width][];
            for (var x = 0; x < Width; x++) {
                grid[x] = new int[Height];
            }

            accessibilityGrids.Add((AccessibilityType) type, grid);
        }
        
        for (var i = 0; i < Width; i++) {
            for (var j = 0; j < Height; j++) {
                UpdateAccessibilityGrids(new IntVec2(i, j));
            }
        }
    }

    public void UpdateAccessibilityGrids(IntVec2 tile) {
        if (!IsPositionInMap(tile)) return;
        foreach(var (type, grid) in accessibilityGrids) {
            grid[tile.X][tile.Y] = (int)CalculateTileCost(tile, type);
        }
    }

    private TileCost CalculateTileCost(IntVec2 tile, AccessibilityType type) {
        Debug.Assert(IsPositionInMap(tile));

        // Solid
        if (tileObjectMap.ContainsKey(tile.ToString()) && tileObjectMap[tile.ToString()].Def.Solid) {
            return TileCost.None;
        }
        
        // Water
        if (Elevation.IsTileWater(tile)) {
            return type switch {
                AccessibilityType.NoWater => TileCost.None,
                AccessibilityType.NoWaterIgnorePaths => TileCost.None,
                AccessibilityType.PathsOnly => TileCost.None,
                _ => TileCost.Normal
            };
        }

        // Path
        if (FootPaths.GetFootPathAtTile(tile)!.Exists) {
            return type switch {
                AccessibilityType.PathsOnly          => TileCost.Normal,
                AccessibilityType.NoSolidIgnorePaths => TileCost.Normal,
                AccessibilityType.NoWaterIgnorePaths => TileCost.Normal,
                _                                    => TileCost.Preferred
            };
        }

        return type switch {
            AccessibilityType.PathsOnly => TileCost.None,
            _                           => TileCost.Normal
        };
    }

    public int GetTileCost(IntVec2 tile, AccessibilityType type) {
        Debug.Assert(isSetup);
        
        return accessibilityGrids[type][tile.X][tile.Y];
    }

    public bool IsTileWalkable(IntVec2 tile, AccessibilityType type) {
        return GetTileCost(tile, type) != (int)TileCost.None;
    }
    
    public bool IsPositionInMap(Vector2 pos) {
        return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
    }

    public IntVec2 GetTileInDirection(IntVec2 tile, Side dir) {
        switch (dir) {
            case Side.North: return new IntVec2(tile.X, tile.Y - 1);
            case Side.East:  return new IntVec2(tile.X + 1, tile.Y);
            case Side.South: return new IntVec2(tile.X, tile.Y + 1);
            case Side.West:  return new IntVec2(tile.X - 1, tile.Y);
            default:         return tile;
        }
    }

    public IEnumerable<IntVec2> GetAdjacentTiles(IntVec2 tile, bool diagonals = false) {
        if (!IsPositionInMap(tile)) yield break;
        
        for (var i = 0; i < 4; i++) {
            var adj = GetTileInDirection(tile, (Side)i);
            if (IsPositionInMap(adj)) yield return adj;
        }

        if (!diagonals) yield break;
        
        for (var i = 0; i < 4; i++) {
            var adj  = GetTileInDirection(tile, (Side)i);
            var adj2 = GetTileInDirection(adj, (Side)((i + 1) % 4));
            if (IsPositionInMap(adj2)) yield return adj2;
        }
    }

    public IEnumerable<IntVec2> GetAccessibleAdjacentTiles(IntVec2 tile) {
        if (!IsPositionInMap(tile)) yield break;
        
        for (var i = 0; i < 4; i++) {
            if (Walls.GetWallAtTile(tile, (Side)i) is { Exists: true }) continue;
            var adj = GetTileInDirection(tile, (Side)i);
            if (!IsPositionInMap(adj)) continue;
            yield return adj;
        }
    }

    public static Side GetQuadrantAtPos(Vector2 pos) {
        var xRel = (pos.X + LargerThanWorld) % 1f - 0.5f;
        var yRel = (pos.Y + LargerThanWorld) % 1f - 0.5f;

        if (yRel <= 0 && Math.Abs(yRel) >= Math.Abs(xRel)) return Side.North;
        if (xRel >= 0 && Math.Abs(xRel) >= Math.Abs(yRel)) return Side.East;
        if (yRel >= 0 && Math.Abs(yRel) >= Math.Abs(xRel)) return Side.South;
        if (xRel <= 0 && Math.Abs(xRel) >= Math.Abs(yRel)) return Side.West;

        return Side.North;
    }

    public IEnumerable<Entity> GetEntitiesAtTile(IntVec2 tile) {
            if (!IsPositionInMap(tile)) yield break;
        
        if (entitiesByTileStatic.ContainsKey(tile.ToString())) {
            foreach (var entity in entitiesByTileStatic[tile.ToString()]) {
                if (entity.Despawned) continue;
                
                yield return entity;
            }
        }

        if (entitiesByTileDynamic.ContainsKey(tile.ToString())) {
            foreach (var entity in entitiesByTileDynamic[tile.ToString()]) {
                if (entity.Despawned) continue;
                
                yield return entity;
            }
        }
    }

    public TileObject? GetTileObjectAtTile(IntVec2 tile) {
        return tileObjectMap.TryGetValue(tile.ToString(), out var tileObject) ? tileObject : null;
    }

    private void RenderDebugCellGrid() {
        if (!isSetup) return;
        
        for (var i = 0; i < Height + 1; i++) {
            Debug.DrawLine(
                new Vector2(0, i),
                new Vector2(Height, i),
                Color.WHITE, 
                true
            );
        }
        // Vertical
        for (var i = 0; i < Width + 1; i++) {
            Debug.DrawLine(
                new Vector2(i, 0),
                new Vector2(i, Width),
                Color.WHITE, 
                true
            );
        }
    }

    public void Serialise() {
        // TODO Do we need to properly reset and setup here
        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            entitiesByTileStatic.Clear();
            tileObjectMap.Clear();
            
            Areas.Reset();
            Exhibits.Reset();
        }
            
        Find.SaveManager.ArchiveValue("width", ref Width);
        Find.SaveManager.ArchiveValue("height", ref Height);
        
        // Re setup pathfinding grid with new width & height
        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            Pathfinder = new Pathfinder(Width, Height);
        }
        
        Find.SaveManager.ArchiveDeep("elevation", Elevation);
        Find.SaveManager.ArchiveDeep("terrain", Terrain);
        Find.SaveManager.ArchiveDeep("walls", Walls);
        Find.SaveManager.ArchiveDeep("footpaths", FootPaths);

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            Areas.Setup(Find.Zoo.Entrance);
            Exhibits.Setup();
            PopulateAccessibilityGrids();
        }
        
        Find.SaveManager.ArchiveDeep("exhibits", Exhibits);
    }

    public void PostLoad() {
        Exhibits.UpdateAllExhibitCaches();
    }
}