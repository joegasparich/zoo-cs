using System.Numerics;
using Raylib_cs;
using Zoo.entities;

namespace Zoo;

public static class Game {
    // TODO: Config options
    private const int MsPerUpdate  = 10;
    public const  int ScreenWidth  = 1280;
    public const  int ScreenHeight = 720;

    // Managers
    public static Renderer     Renderer = new ();
    public static AssetManager AssetManager = new ();
    
    // Collections
    private static Dictionary<int, Entity> entities = new ();
    private static List<Entity>            entitiesToAdd = new ();
    private static List<Entity>            entitiesToRemove = new ();
    
    // State TODO: Save these
    private static int ticksSinceGameStart;
    private static int framesSinceGameStart;
    private static int nextEntityId;
    
    public static void Run() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Application Started");
        
        Init();

        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Application Loaded");
        
        DoLoop();

        Cleanup();
    }

    private static void Init() {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Hello World");

        AssetManager.LoadAssets();

        // Test
        var testEntity = new Entity(new Vector2(0, 0));
        var renderer = testEntity.AddComponent<RenderComponent>();
        renderer.SetSprite(Assets.Keeper);
        RegisterEntity(testEntity);
    }

    private static void Cleanup() {
        Raylib.CloseWindow();
    }

    private static void DoLoop() {
        var    lastTime = Raylib.GetTime() * 1000;
        double lag      = 0;
        
        while (!Raylib.WindowShouldClose()) {
            var currentTime = Raylib.GetTime() * 1000;
            var elapsed     = currentTime - lastTime;
            
            lastTime =  currentTime;
            lag      += elapsed;

            while (lag >= MsPerUpdate) {
                // Do Update
                PreUpdate();
                Update();
                PostUpdate();
                
                lag -= MsPerUpdate;
                ticksSinceGameStart++;
            }

            Render();
        }
    }
    
    private static void PreUpdate() {
        foreach (var entity in entities.Values) {
            entity.PreUpdate();
        }
    }
    
    private static void Update() {
        Renderer.Update();
        
        foreach (var entity in entities.Values) {
            entity.Update();
        }
    }
    
    private static void PostUpdate() {
        foreach (var entity in entities.Values) {
            entity.PostUpdate();
        }

        foreach (var entity in entitiesToAdd) {
            entity.Setup();
            entities.Add(entity.Id, entity);
        }
        entitiesToAdd.Clear();
        
        foreach (var entity in entitiesToRemove) {
            entities.Remove(entity.Id);
        }
        entitiesToRemove.Clear();
    }

    private static void Render() {
        Renderer.BeginDrawing();
        {
            foreach (var entity in entities.Values) {
                entity.Render();
            }
        }
        Renderer.EndDrawing();

        framesSinceGameStart++;
    }

    public static int RegisterEntity(Entity entity) {
        return RegisterEntity(entity, nextEntityId++);
    }

    public static int RegisterEntity(Entity entity, int id) {
        entity.Id = id;
        entitiesToAdd.Add(entity);
        
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, $"Registered entity {entity.Id}");

        return id;
    }
    
    public static void UnregisterEntity(Entity entity) {
        entitiesToRemove.Add(entity);
    }
    
}