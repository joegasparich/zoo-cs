using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.world;

public enum Biome : byte {
    Grass,
    Sand,
    Snow
}

public struct BiomeInfo {
    public string Name;
    public Color Color;

    public static BiomeInfo Get(Biome biome) {
        switch (biome) {
            case Biome.Grass: return new() { Name = "Grass", Color = new Color(182, 213, 60,  255) };
            case Biome.Sand:  return new() { Name = "Sand", Color  = new Color(244, 204, 161, 255) };
            case Biome.Snow:  return new() { Name = "Snow", Color  = new Color(223, 246, 246, 255) };
            default:          return new();
        }
    }
}

public class BiomeGrid {
    public const   int   BiomeScale          = 2;
    internal const int   ChunkSize           = 5;
    private const  float SlopeColourStrength = 0.3f;

    private int            rows;
    private int            cols;
    private bool           isSetup = false;
    private BiomeChunk[,] chunkGrid;

    public BiomeGrid(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;
        
        var chunkCols = cols / ChunkSize + Convert.ToInt32(cols % ChunkSize != 0);
        var chunkRows = rows / ChunkSize + Convert.ToInt32(rows % ChunkSize != 0);
        
        chunkGrid = new BiomeChunk[chunkCols, chunkRows];
    }

    public void Setup() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Setting up biome grid");
        
        for (var i = 0; i < chunkGrid.GetLength(0); i++) {
            for (var j = 0; j < chunkGrid.GetLength(1); j++) {
                chunkGrid[i, j] = new BiomeChunk(
                    i, 
                    j,
                    i == chunkGrid.GetLength(0) - 1 && cols % ChunkSize != 0 ? cols & ChunkSize : ChunkSize,
                    j == chunkGrid.GetLength(1) - 1 && rows % ChunkSize != 0 ? rows & ChunkSize : ChunkSize
                );
            }
        }
        
        // elevationListenerHandle = Messenger::on(EventType::ElevationUpdated, [this](Event * _e) {
        //     auto e = static_cast<ElevationUpdatedEvent*>(_e);
        //
        //     regenerateChunksInRadius(e->pos * 2.0f, e->radius + 6.0f);
        // });

        isSetup = true;
    }

    public void Reset() {
        cols      = 0;
        rows      = 0;
        chunkGrid = null;
        
        // Messenger::unsubscribe(EventType::ElevationUpdated, elevationListenerHandle);
        // elevationListenerHandle = "";

        isSetup = false;
    }

    public void PostUpdate() {
        foreach (var chunk in chunkGrid) {
            chunk.PostUpdate();
        }
    }

    public void Render() {
        foreach (var chunk in chunkGrid) {
            chunk.Render();
        }
    }

    private bool IsChunkInGrid(int col, int row) {
        return col >= 0 && col < cols / ChunkSize && row >= 0 && row < rows / ChunkSize;
    }
    
    private BiomeChunk GetChunk(int col, int row) {
        if (!IsChunkInGrid(col, row)) return null;
        return chunkGrid[col, row];
    }

    private IEnumerable<BiomeChunk> GetChunksInRadius(Vector2 pos, float radius) {
        var floorX = JMath.FloorToInt(pos.X / ChunkSize);
        var floorY = JMath.FloorToInt(pos.Y / ChunkSize);

        // TODO: Only handles 3x3 around the pos. Change this to calculate required area based on radius
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
                    yield return chunkGrid[floorX + i, floorY + j];
                }
            }
        }
    }

    public void SetBiomeInRadius(Vector2 pos, float radius, Biome biome) {
        foreach (var chunk in GetChunksInRadius(pos, radius)) {
            chunk.SetBiomeInRadius(pos - new Vector2(chunk.X * ChunkSize, chunk.Y * ChunkSize), radius, biome);
        }
    }

    public void RegenerateChunksInRadius(Vector2 pos, float radius) {
        foreach (var chunk in GetChunksInRadius(pos, radius)) {
            chunk.ShouldRegenerate = true;
        }
    }

    public void RegenerateAllChunks() {
        foreach (var chunk in chunkGrid) {
            chunk.ShouldRegenerate = true;
        }
    }
}

internal class BiomeChunk : IDisposable {
    private Mesh          chunkMesh;
    private BiomeCell[][] grid;
    private bool          isSetup = false;
    
    public bool ShouldRegenerate { get; set; } = false;
    public int  X                { get; }
    public int  Y                { get; }
    public int  Rows             { get; }
    public int  Cols             { get; }
    
    public BiomeChunk(int x, int y, int rows, int cols) {
        X    = x;
        Y    = y;
        Rows = rows;
        Cols = cols;
        
        grid = new BiomeCell[Cols][];
        for (int i = 0; i < Cols; i++) {
            grid[i] = new BiomeCell[Rows];
            for (int j = 0; j < Rows; j++) {
                grid[i][j] = new BiomeCell(new []{ Biome.Grass, Biome.Grass, Biome.Grass, Biome.Grass});
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

        RegenerateMesh();
    }
    
    public void PostUpdate() {
        if (ShouldRegenerate) {
            // TODO: Investigate drawing one chunk per frame or something
            RegenerateMesh();
            ShouldRegenerate = false;
        }
    }

    private void RegenerateMesh() {
        int vertexIndex = 0;
        
        var vertices = new float[chunkMesh.vertexCount * 3];
        var colours  = new byte[chunkMesh.vertexCount  * 4];

        for (int i = 0; i < Cols; ++i) {
            for (int j = 0; j < Rows; ++j) {
                var cell = grid[i][j];
                for (int q = 0; q < 4; q++) {
                    var colour = BiomeInfo.Get(cell.Quadrants[q]).Color;
                    // TODO: Slope colour
                    
                    var quadrantVertices = BiomeCell.GetQuadrantVertices(new Vector2(i, j), (Side)q);
                    for (int v = 0; v < 3; v++) {
                        // TODO: Elevation
                        
                        vertices[vertexIndex * 3]     = quadrantVertices[v].X;
                        vertices[vertexIndex * 3 + 1] = quadrantVertices[v].Y;
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
        if (!Find.Renderer.IsRectangleOnScreen(new Rectangle(
            X * BiomeGrid.ChunkSize / (float)BiomeGrid.BiomeScale,
            Y * BiomeGrid.ChunkSize / (float)BiomeGrid.BiomeScale,
            Rows                    / (float)BiomeGrid.BiomeScale,
            Cols                    / (float)BiomeGrid.BiomeScale))
        ) return;

        var matDefault = Raylib.LoadMaterialDefault();
        Raylib.DrawMesh(chunkMesh, matDefault, Matrix4x4.Transpose(
            Matrix4x4.CreateTranslation(X * BiomeGrid.ChunkSize, Y * BiomeGrid.ChunkSize, Depth.Ground.ToInt()) *
            Matrix4x4.CreateScale(Renderer.WorldScale / (float)BiomeGrid.BiomeScale, Renderer.WorldScale / (float)BiomeGrid.BiomeScale, 1)
        ));
    }

    public void Dispose() {
        Raylib.UnloadMesh(ref chunkMesh);
    }

    public void SetBiomeInRadius(Vector2 pos, float radius, Biome biome) {
        bool changed = false;
        
        for (float i = pos.X - radius; i < pos.X + radius; i++) {
            for (float j = pos.Y - radius; j < pos.Y + radius; j++) {
                var cellPos = new Vector2(MathF.Floor(i), MathF.Floor(j));
                if (!IsPositionInChunk(cellPos)) continue;

                // Get triangles in circle
                for (var q = 0; q < 4; q++) {
                    var side = (Side)q;
                    foreach (var point in BiomeCell.GetQuadrantVertices(cellPos, side)) {
                        if (JMath.PointInCircle(pos, radius, point)) {
                            var xFloor = JMath.RoundToInt(cellPos.X);
                            var yFloor = JMath.RoundToInt(cellPos.Y);
                            if (grid[xFloor][yFloor].Quadrants[(int)side] != biome) {
                                grid[xFloor][yFloor].SetQuadrant(side, biome);
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        if (changed) {
            RegenerateMesh();
        }
    }

    public bool IsPositionInChunk(Vector2 pos) {
        return pos.X >= 0 && pos.X < Cols && pos.Y >= 0 && pos.Y < Rows;
    }
}

internal struct BiomeCell {
    public Biome[] Quadrants;
    
    public BiomeCell(Biome[] quadrants) {
        Quadrants = quadrants;
    }
    
    public void SetQuadrant(Side quadrant, Biome biome) {
        Quadrants[quadrant.ToInt()] = biome;
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