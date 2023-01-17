using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Raylib_cs;
using Zoo.defs;
using Zoo.util;

namespace Zoo.world;

public class TerrainGrid : ISerialisable {
    // Constants
    public static readonly int   TerrainScale          = 2;
    internal const         int   ChunkSize           = 20;
    internal const         float SlopeColourStrength = 0.3f;

    // Config
    private int rows;
    private int cols;
    
    // Collections
    private  TerrainChunk[][]    chunkGrid;
    internal Queue<TerrainChunk> chunkRegenQueue = new();
    
    // State
    private bool           isSetup = false;
    private string         elevationListenerHandle;

    public TerrainGrid(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;
    }

    public void Setup(string[][][][][]? data = null) {
        if (isSetup) {
            Debug.Warn("Tried to setup TerrainGrid which was already setup");
            return;
        }
        
        Debug.Log("Setting up terrain grid");
        
        var chunkCols = cols / ChunkSize + Convert.ToInt32(cols % ChunkSize != 0);
        var chunkRows = rows / ChunkSize + Convert.ToInt32(rows % ChunkSize != 0);
        
        chunkGrid = new TerrainChunk[chunkCols][];
        
        for (var i = 0; i < chunkCols; i++) {
            chunkGrid[i] = new TerrainChunk[chunkRows];
            
            for (var j = 0; j < chunkRows; j++) {
                chunkGrid[i][j] = new TerrainChunk(
                    i, 
                    j,
                    i == chunkCols - 1 && cols % ChunkSize != 0 ? cols % ChunkSize : ChunkSize,
                    j == chunkRows - 1 && rows % ChunkSize != 0 ? rows % ChunkSize : ChunkSize,
                    data?[i][j]
                );
            }
        }
        
        elevationListenerHandle = Messenger.On(EventType.ElevationUpdated, OnElevationUpdated);

        isSetup = true;
    }

    public void Reset() {
        if (!isSetup) {
            Debug.Warn("Tried to reset when TerrainGrid wasn't setup");
            return;
        }
        
        cols      = 0;
        rows      = 0;
        chunkGrid = null;
        
        Messenger.Off(EventType.ElevationUpdated, elevationListenerHandle);
        elevationListenerHandle = "";

        isSetup = false;
    }

    public void PostUpdate() {
        if (chunkRegenQueue.Count > 0) {
            var chunkToRegen = chunkRegenQueue.Dequeue();
            chunkToRegen.RegenerateMeshNow();
        }
    }

    public void Render() {
        foreach (var row in chunkGrid) {
            foreach (var chunk in row) {
                chunk.Render();
            }
        }
    }

    private bool IsChunkInGrid(int col, int row) {
        return col >= 0 && col < cols / (float)ChunkSize && row >= 0 && row < rows / (float)ChunkSize;
    }
    
    public TerrainChunk GetChunk(int col, int row) {
        if (!IsChunkInGrid(col, row)) return null;
        return chunkGrid[col][row];
    }

    public TerrainChunk GetChunkAtTile(IntVec2 pos) {
        var col = pos.X * TerrainScale / ChunkSize;
        var row = pos.Y * TerrainScale / ChunkSize;
        return GetChunk(col, row);
    }

    public IEnumerable<TerrainChunk> GetChunksInRadius(Vector2 pos, float radius) {
        var floorX = (pos.X / ChunkSize).FloorToInt();
        var floorY = (pos.Y / ChunkSize).FloorToInt();

        // Assumes that radius will never be larger than ChunkSize * 2
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (IsChunkInGrid(floorX + i, floorY + j)
                    && JMath.CircleIntersectsRect(
                        new Vector2((floorX + i) * ChunkSize, (floorY + j) * ChunkSize),
                        new Vector2(ChunkSize, ChunkSize),
                        pos,
                        radius
                    )
                ) {
                    yield return chunkGrid[floorX + i][floorY + j];
                }
            }
        }
    }

    public void SetTerrainInRadius(Vector2 pos, float radius, TerrainDef terrain) {
        foreach (var chunk in GetChunksInRadius(pos, radius)) {
            chunk.SetTerrainInRadius(pos - new Vector2(chunk.X * ChunkSize, chunk.Y * ChunkSize), radius, terrain);
        }
    }
    
    private void OnElevationUpdated(object data) {
        var (pos, radius) = (ValueTuple<Vector2, float>)data;
        RegenerateChunksInRadius(pos * TerrainScale, radius + 6);
    }

    public void RegenerateChunksInRadius(Vector2 pos, float radius) {
        foreach (var chunk in GetChunksInRadius(pos, radius)) {
            chunkRegenQueue.Enqueue(chunk);
        }
    }

    public void RegenerateAllChunks() {
        foreach (var row in chunkGrid) {
            foreach (var chunk in row) {
                chunkRegenQueue.Enqueue(chunk);
            }
        }
    }
    
    public void RenderChunkDebug() {
        if (!isSetup) return;
        // Horizontal
        for (var i = 0; i < (rows / ChunkSize) + 1; i++) {
            Debug.DrawLine(
                new Vector2(0, i * ChunkSize / (float)TerrainScale),
                new Vector2(Find.World.Height, i * ChunkSize / (float)TerrainScale),
                Color.ORANGE, 
                true
            );
        }
        // Vertical
        for (var i = 0; i < (cols / ChunkSize) + 1; i++) {
            Debug.DrawLine(
                new Vector2(i * ChunkSize / (float)TerrainScale, 0),
                new Vector2(i * ChunkSize / (float)TerrainScale, Find.World.Width),
                Color.ORANGE, 
                true
            );
        }
    }

    public void Serialise() {
        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            Reset();
        
        Find.SaveManager.ArchiveValue("cols", ref cols);
        Find.SaveManager.ArchiveValue("rows", ref rows);

        Find.SaveManager.ArchiveValue("data", 
            () => chunkGrid.Select(row => row.Select(chunk => chunk.Save()).ToArray()).ToArray(), 
            chunkData => Setup(chunkData)
        );
    }
}

public class TerrainChunk : IDisposable {
    private Mesh          chunkMesh;
    private TerrainCell[][] grid;
    private bool          isSetup          = false;
    private bool          shouldRegenerate = false;
    
    public int  X                { get; }
    public int  Y                { get; }
    public int  Rows             { get; }
    public int  Cols             { get; }

    public Vector2 ChunkPos => new Vector2(X * TerrainGrid.ChunkSize, Y * TerrainGrid.ChunkSize);
    
    public TerrainChunk(int x, int y, int cols, int rows, string[][][]? data = null) {
        X    = x;
        Y    = y;
        Rows = rows;
        Cols = cols;

        grid = new TerrainCell[Cols][];
        for (var i = 0; i < Cols; i++) {
            grid[i] = new TerrainCell[Rows];
            for (var j = 0; j < Rows; j++) {
                if (data != null) {
                    grid[i][j] = new TerrainCell(data[i][j].Select(id => Find.AssetManager.GetDef<TerrainDef>(id)).ToArray());
                } else {
                    grid[i][j] = new TerrainCell(new []{ TerrainDefOf.Grass, TerrainDefOf.Grass, TerrainDefOf.Grass, TerrainDefOf.Grass});
                }
            }
        }

        chunkMesh               = new Mesh();
        chunkMesh.triangleCount = cols * rows * 4;
        chunkMesh.vertexCount   = chunkMesh.triangleCount * 3;
        
        var vertices = new float[chunkMesh.vertexCount * 3];
        var colors = new byte[chunkMesh.vertexCount * 4];
        
        unsafe {
            fixed (float* ptr = &vertices[0]) {
                chunkMesh.vertices = ptr;
            }
            fixed (byte* ptr = colors) {
                chunkMesh.colors = ptr;
            }
        }
        
        Raylib.UploadMesh(ref chunkMesh, true);

        isSetup = true;

        RegenerateMeshNow();
    }
    
    public void Regenerate() {
        Find.World.Terrain.chunkRegenQueue.Enqueue(this);
    }

    internal void RegenerateMeshNow() {
        int vertexIndex = 0;
        
        var vertices = new float[chunkMesh.vertexCount * 3];
        var colours  = new byte[chunkMesh.vertexCount  * 4];

        for (int i = 0; i < Cols; ++i) {
            for (int j = 0; j < Rows; ++j) {
                var cell = grid[i][j];
                for (int q = 0; q < 4; q++) {
                    var colour          = cell.Quadrants[q].Colour;
                    var pos             = ChunkPos + new Vector2(i, j);
                    var tile            = (pos / TerrainGrid.TerrainScale).Floor();
                    var slopeBrightness = ElevationUtility.GetSlopeVariantColourOffset(Find.World.Elevation.GetTileSlopeVariant(tile), pos, (Side)q);
                    colour = colour.Brighten(slopeBrightness * TerrainGrid.SlopeColourStrength);
                    
                    var quadrantVertices = TerrainCell.GetQuadrantVertices(new Vector2(i, j), (Side)q);
                    for (int v = 0; v < 3; v++) {
                        var elevation = Find.World.Elevation.GetElevationAtPos((quadrantVertices[v] + ChunkPos) / TerrainGrid.TerrainScale);
                        
                        vertices[vertexIndex * 3]     = quadrantVertices[v].X;
                        vertices[vertexIndex * 3 + 1] = quadrantVertices[v].Y - elevation * TerrainGrid.TerrainScale;
                        vertices[vertexIndex * 3 + 2] = 0;
                        
                        colours[vertexIndex * 4]     = colour.r;
                        colours[vertexIndex * 4 + 1] = colour.g;
                        colours[vertexIndex * 4 + 2] = colour.b;
                        colours[vertexIndex * 4 + 3] = colour.a;
                        
                        vertexIndex++;
                    }
                }
            }
        }

        unsafe {
            fixed (void* p = &vertices[0]) {
                chunkMesh.vertices = (float*)p;
                Raylib.UpdateMeshBuffer(chunkMesh, 0, p, chunkMesh.vertexCount * 3 * sizeof(float), 0);
            }
            fixed (void* p = &colours[0]) {
                chunkMesh.colors = (byte*)p;
                Raylib.UpdateMeshBuffer(chunkMesh, 3, p, chunkMesh.vertexCount * 4 * sizeof(byte), 0);
            }
        }
    }

    public void Render() {
        // Cull offscreen chunks
        if (!Find.Renderer.IsWorldRectOnScreen(new Rectangle(
            X * TerrainGrid.ChunkSize / (float)TerrainGrid.TerrainScale,
            Y * TerrainGrid.ChunkSize / (float)TerrainGrid.TerrainScale,
            Rows                    / (float)TerrainGrid.TerrainScale,
            Cols                    / (float)TerrainGrid.TerrainScale))
        ) return;

        var matDefault = Raylib.LoadMaterialDefault();
        Raylib.DrawMesh(chunkMesh, matDefault, Matrix4x4.Transpose(
            Matrix4x4.CreateTranslation(X * TerrainGrid.ChunkSize, Y * TerrainGrid.ChunkSize, (int)Depth.Ground) *
            Matrix4x4.CreateScale(World.WorldScale / (float)TerrainGrid.TerrainScale, World.WorldScale / (float)TerrainGrid.TerrainScale, 1)
        ));
    }

    public void Dispose() {
        Raylib.UnloadMesh(ref chunkMesh);
    }

    public void SetTerrainInRadius(Vector2 pos, float radius, TerrainDef terrain) {
        bool changed = false;
        
        for (float i = pos.X - radius; i < pos.X + radius; i++) {
            for (float j = pos.Y - radius; j < pos.Y + radius; j++) {
                var cellPos = new Vector2(MathF.Floor(i), MathF.Floor(j));
                if (!IsPositionInChunk(cellPos)) continue;

                // Get triangles in circle
                for (var q = 0; q < 4; q++) {
                    var side = (Side)q;
                    foreach (var point in TerrainCell.GetQuadrantVertices(cellPos, side)) {
                        if (JMath.PointInCircle(pos, radius, point)) {
                            var xFloor = cellPos.X.RoundToInt();
                            var yFloor = cellPos.Y.RoundToInt();
                            if (grid[xFloor][yFloor].Quadrants[(int)side] != terrain) {
                                grid[xFloor][yFloor].SetQuadrant(side, terrain);
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        if (changed) {
            Regenerate();
        }
    }

    public bool IsPositionInChunk(Vector2 pos) {
        return pos.X >= 0 && pos.X < Cols && pos.Y >= 0 && pos.Y < Rows;
    }

    public string[][][] Save() {
        return grid.Select(row => row.Select(cell => cell.Quadrants.Select(terrain => terrain.Id).ToArray()).ToArray()).ToArray();
    }
    
    public void Load(string[][][] data) {
        for (var i = 0; i < Cols; ++i) {
            for (var j = 0; j < Rows; ++j) {
                grid[i][j].Quadrants = data[i][j].Select(id => Find.AssetManager.GetDef<TerrainDef>(id)).ToArray();
            }
        }
        
        RegenerateMeshNow();
    }
}

internal class TerrainCell {
    public TerrainDef[] Quadrants;
    
    public TerrainCell(TerrainDef[] quadrants) {
        Quadrants = quadrants;
    }
    
    public void SetQuadrant(Side quadrant, TerrainDef terrain) {
        Quadrants[(int)quadrant] = terrain;
    }

    public static Vector2[] GetQuadrantVertices(Vector2 pos, Side quadrant) {
        switch (quadrant) {
            case Side.North: return new Vector2[] { new(pos.X, pos.Y), new(pos.X + 0.5f, pos.Y + 0.5f), new(pos.X + 1, pos.Y) };
            case Side.West:  return new Vector2[] { new(pos.X, pos.Y), new(pos.X, pos.Y + 1), new(pos.X + 0.5f, pos.Y + 0.5f) };
            case Side.South: return new Vector2[] { new(pos.X, pos.Y + 1), new(pos.X + 1, pos.Y + 1), new(pos.X + 0.5f, pos.Y + 0.5f) };
            case Side.East:  return new Vector2[] { new(pos.X + 1, pos.Y), new(pos.X + 0.5f, pos.Y + 0.5f), new(pos.X + 1, pos.Y + 1) };
            default: return Array.Empty<Vector2>();
        }
    }
}