using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.util;
using Zoo.world;

namespace Zoo; 

public class Scene_Zoo : Scene {
    // Constants
    private const string SceneName = "Zoo";
    
    // State
    public Zoo Zoo;

    public Scene_Zoo() : base(SceneName) {
        Zoo = new Zoo();
    }

    public override void Start() {
        Debug.Log("Creating new zoo");
        Zoo.Setup();

        GenerateOuterWalls();
    }

    public override void Stop() {
        base.Stop();
        
        Zoo.Cleanup();
    }

    public override void PreUpdate() {
        Zoo.PreUpdate();
    }
    public override void Update() {
        Zoo.Update();
    }
    public override void PostUpdate() {
        Zoo.PostUpdate();
    }

    public override void Render() {
        Zoo.Render();
    }
    public override void RenderLate() {
        Zoo.RenderLate();
    }

    public override void OnGUI() {
        Zoo.OnGUI();
    }

    public override void OnInput(InputEvent evt) {
        Zoo.OnInput(evt);
    }

    private void GenerateOuterWalls() {
        for (var i = 0; i < Zoo.Width; i++) {
            Zoo.World.Walls.PlaceWallAtTile(WallDefOf.IronBarFence, new IntVec2(i, 0),              Side.North, true);
            Zoo.World.Walls.PlaceWallAtTile(WallDefOf.IronBarFence, new IntVec2(i, Zoo.Height - 1), Side.South, true);
        }
        for (var i = 0; i < Zoo.Height; i++) {
            Zoo.World.Walls.PlaceWallAtTile(WallDefOf.IronBarFence, new IntVec2(0,             i), Side.West, true);
            Zoo.World.Walls.PlaceWallAtTile(WallDefOf.IronBarFence, new IntVec2(Zoo.Width - 1, i), Side.East, true);
        }
        
        // Entrance
        Zoo.World.Walls.PlaceDoor(Zoo.World.Walls.GetWallAtTile(Zoo.Entrance, Side.South));
    }
    
    public override void Serialise() {
        Find.SaveManager.ArchiveDeep("zoo", Zoo);
    }

    public override void PostLoad() {
        Zoo.PostLoad();
    }
}