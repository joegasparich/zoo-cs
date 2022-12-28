using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.ui;

namespace Zoo;

public static class Game {
    // TODO: Config options
    private const int MsPerUpdate  = 10;
    public const  int ScreenWidth  = 1280;
    public const  int ScreenHeight = 720;

    // Managers
    public static InputManager Input        = new();
    public static Renderer     Renderer     = new();
    public static Registry     Registry     = new();
    public static AssetManager AssetManager = new();
    public static SaveManager  SaveManager  = new();
    public static SceneManager SceneManager = new();
    public static UIManager    UI           = new();
    
    // Collections
    private static Dictionary<int, Entity> entities         = new();
    private static List<Entity>            entitiesToAdd    = new();
    private static List<Entity>            entitiesToRemove = new();
    
    // State TODO: Save these
    private static int ticksSinceGameStart;
    private static int framesSinceGameStart;
    private static int nextEntityId;
    
    public static void Run() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Application Started");
        Init();
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Application Loaded");
        DoLoop();
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Application Cleaning Up");
        Cleanup();
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Application Ended");
    }

    private static void Init() {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Zoo");

        AssetManager.LoadAssets();
        UI.Init();
        SaveManager.NewGame();
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
            
            // Do Render
            UI.PreRender();
            Input.ProcessInput();
            Render();
            UI.PostRender();
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
            Renderer.Begin3D();
            {
                SceneManager.GetCurrentScene().Render();
                
                foreach (var entity in entities.Values) {
                    entity.Render();
                }
            }
            Renderer.End3D();
            
            UI.Render();
        }
        Renderer.EndDrawing();

        framesSinceGameStart++;
    }

    public static void OnInput(InputEvent evt) {
        if (!evt.consumed) UI.OnInput(evt);
        if (!evt.consumed) SceneManager.GetCurrentScene().OnInput(evt);
    }

    public static void OnGUI() {
        Find.SceneManager.GetCurrentScene().OnGUI();
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
    
    public static Entity GetEntityById(int id) {
        return entities[id];
    }
}