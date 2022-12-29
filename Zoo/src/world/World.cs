using System.Numerics;
using Raylib_cs;
using Zoo.entities;
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

public class World {
    public const int WorldScale      = 32;
    public const int LargerThanWorld = 10000;

    public int Width  { get; }
    public int Height { get; }

    public ElevationGrid Elevation  { get; }
    public BiomeGrid     Biomes     { get; }
    public WallGrid      Walls      { get; }
    public FootPathGrid  FootPaths  { get; }
    public AreaManager   Areas      { get; }
    public Pathfinder    Pathfinder { get; }

    private Dictionary<int, Entity>    TileObjects   = new();
    private Dictionary<string, Entity> TileObjectMap = new();

    private bool isSetup = false;

    public World(int width, int height) {
        Width  = width;
        Height = height;

        Elevation = new ElevationGrid(Width, Height);
        Biomes    = new BiomeGrid(Width * BiomeGrid.BiomeScale, Height * BiomeGrid.BiomeScale);
        Walls     = new WallGrid(Width, Height);
        FootPaths = new FootPathGrid(Width, Height);
        Areas     = new AreaManager();
        Pathfinder = new Pathfinder(Width, Height);
    }

    public void Setup() {
        if (isSetup) return;

        Elevation.Setup();
        Biomes.Setup();
        Walls.Setup();
        FootPaths.Setup();
        Areas.Setup();

        isSetup = true;

        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "World setup complete");
    }
    
    public void Cleanup() {
        Elevation.Reset();
        Biomes.Reset();
        Walls.Reset();
        FootPaths.Reset();
        Areas.Reset();

        TileObjects.Clear();
        TileObjectMap.Clear();
        
        isSetup = false;
    }

    public void PreUpdate() {}
    public void Update() {}

    public void PostUpdate() {
        Biomes.PostUpdate();
    }

    public void Render() {
        Biomes.Render();
        Elevation.Render();
        Walls.Render();
        FootPaths.Render();
    }
    public void RenderDebug() {}

    public void RegisterTileObject(Entity tileObject) {
        var component = tileObject.GetComponent<TileObjectComponent>();

        TileObjects.Add(tileObject.Id, tileObject);
        foreach (var tile in component.GetOccupiedTiles()) {
            TileObjectMap.Add(tile.ToString(), tileObject);
        }

        if (component.Data.Solid) {
            Messenger.Fire(EventType.PlaceSolid, component.GetOccupiedTiles().ToList());
        }
    }

    public void UnregisterTileObject(Entity tileObject) {
        var component = tileObject.GetComponent<TileObjectComponent>();

        TileObjects.Remove(tileObject.Id);
        foreach (var tile in component.GetOccupiedTiles()) {
            TileObjectMap.Remove(tile.ToString());
        }
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

    public Entity? GetTileObjectAtTile(IntVec2 tile) {
        return TileObjectMap.TryGetValue(tile.ToString(), out var tileObject) ? tileObject : null;
    }
    
    // TODO (optimisation): cache tile cost grids (paths, no water, etc.)
    // TODO: Set up consts for tile costs
    public int GetTileWalkability(IntVec2 tile) {
        if (!isSetup) return 0;
        if (!IsPositionInMap(tile)) return 0;
        if (TileObjectMap.ContainsKey(tile.ToString()) && TileObjectMap[tile.ToString()].GetComponent<TileObjectComponent>()!.Data.Solid) return 0;
        if (Elevation.IsTileWater(tile)) return 0;
        if (FootPaths.GetPathAtTile(tile)!.Exists) return 1;

        return 3;
    }
}