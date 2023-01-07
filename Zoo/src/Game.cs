using Raylib_cs;
using Zoo.entities;
using Zoo.ui;

namespace Zoo;

public static class Game {
    // Enums
    private enum TickRate {
        Normal = 1,
        Fast = 2,
        Faster = 4
    }
    
    // Constants
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
    
    // State
    private static int      ticksSinceGameStart;
    private static int      framesSinceGameStart;
    private static int      nextEntityId = 1;
    private static bool     paused;
    private static TickRate tickRate = TickRate.Normal;

    // Properties
    public static int Ticks => ticksSinceGameStart;
    public static int Frames => framesSinceGameStart;
    
    public static void Run() {
        Debug.Log("Application Started");
        Init();
        Debug.Log("Application Loaded");
        DoLoop();
        Debug.Log("Application Cleaning Up");
        Cleanup();
        Debug.Log("Application Ended");
    }

    private static void Init() {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "Zoo");
        Raylib.SetExitKey(KeyboardKey.KEY_NULL);
        
        AssetManager.LoadAssets();
        UI.Init();
        Find.SceneManager.LoadScene(new Scene_Menu());
    }

    private static void Cleanup() {
        Raylib.CloseWindow();
    }

    private static void DoLoop() {
        var    lastTime = Raylib.GetTime() * 1000;
        double lag      = 0;
        
        while (!Raylib.WindowShouldClose()) {
            var currentTime = Raylib.GetTime() * 1000;

            if (!paused) {
                var elapsed = currentTime - lastTime;
                lag += elapsed;
                
                var msPerUpdate = MsPerUpdate / (int) tickRate;
                
                while (lag >= msPerUpdate) {
                    // Do Update
                    PreUpdate();
                    Update();
                    PostUpdate();
                    
                    lag -= msPerUpdate;
                    ticksSinceGameStart++;
                }
            }
            
            lastTime = currentTime;
            
            // Do Render
            UI.PreRender();
            Input.ProcessInput();
            Renderer.Render();
            UI.PostRender();
        }
    }
    
    private static void PreUpdate() {
        SceneManager.GetCurrentScene().PreUpdate();
        foreach (var entity in entities.Values) {
            entity.PreUpdate();
        }
    }
    
    private static void Update() {
        Renderer.Update();
        
        SceneManager.GetCurrentScene().Update();
        foreach (var entity in entities.Values) {
            entity.Update();
        }
    }
    
    private static void PostUpdate() {
        SceneManager.GetCurrentScene().PostUpdate();
        foreach (var entity in entities.Values) {
            entity.PostUpdate();
        }

        foreach (var entity in entitiesToAdd) {
            try {
                entity.Setup();
                entities.Add(entity.Id, entity);
            }
            catch (Exception e) {
                Debug.Error($"Failed to set up entity {entity.Id}:", e);
                entity.Destroy();
            }
        }
        entitiesToAdd.Clear();   
        
        foreach (var entity in entitiesToRemove) {
            entities.Remove(entity.Id);
        }
        entitiesToRemove.Clear();
    }

    public static void Render3D() {
        SceneManager.GetCurrentScene().Render();
        
        foreach (var entity in entities.Values) {
            entity.Render();
        }
        
        SceneManager.GetCurrentScene().RenderLate();

        framesSinceGameStart++;
    }

    public static void Render2D() {
        UI.Render();
    }

    public static void OnInput(InputEvent evt) {
        if (!evt.consumed) UI.OnInput(evt);
        
        if (!evt.consumed) SceneManager.GetCurrentScene().OnInput(evt);
        
        foreach (var entity in entities.Values) {
            if (!evt.consumed) entity.OnInput(evt);
        }

        // Tick rate controls
        if (!evt.consumed) {
            if (evt.inputDown == InputType.Pause) {
                paused = !paused;
                evt.Consume();
            }
            if (evt.inputDown == InputType.NormalSpeed) {
                tickRate = TickRate.Normal;
                evt.Consume();
            }
            if (evt.inputDown == InputType.FastSpeed) {
                tickRate = TickRate.Fast;
                evt.Consume();
            }
            if (evt.inputDown == InputType.FasterSpeed) {
                tickRate = TickRate.Faster;
                evt.Consume();
            }
        }
        
        if (!evt.consumed) Renderer.OnInput(evt);
        
        UI.PostInput(evt);
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
        
        Debug.Log($"Registered entity {entity.Id}");

        return id;
    }
    
    public static void UnregisterEntity(Entity entity) {
        entitiesToRemove.Add(entity);
    }
    
    public static Entity GetEntityById(int id) {
        return entities[id];
    }

    public static void ClearEntities() {
        foreach (var entity in entities) {
            entity.Value.Destroy();
        }
        foreach (var entity in entitiesToAdd) {
            entity.Destroy();
        }
        
        entities.Clear();
        entitiesToAdd.Clear();
        entitiesToRemove.Clear();
    }

    public static void Serialise() {
        SaveManager.ArchiveValue("ticksSinceGameStart",  ref ticksSinceGameStart);
        SaveManager.ArchiveValue("framesSinceGameStart", ref framesSinceGameStart);
        SaveManager.ArchiveValue("nextEntityId",         ref nextEntityId);
        
        // TODO (fix): probably need to clear temp entity lists
        if (SaveManager.Mode == SerialiseMode.Loading)
            entities.Clear();
            // TODO: do we need to reset entities and components? Probably not
            
        SaveManager.ArchiveDeep("scene", SceneManager.GetCurrentScene());
        SaveManager.ArchiveCustom("entities", 
            () => EntityUtility.SaveEntities(entities.Values),
            data => EntityUtility.LoadEntities(data.AsArray())
        );
    }
}