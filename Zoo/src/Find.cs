using Zoo.ui;
using Zoo.world;

namespace Zoo; 

public static class Find {
    public static Zoo Zoo {
        get {
            var scene = Game.SceneManager.GetCurrentScene();
            if (scene.Name == "Zoo") {
                return ((ZooScene)scene).Zoo;
            }
            return null!;
        }
    }

    public static World World => Zoo.World;

    public static Registry     Registry     => Game.Registry;
    public static InputManager Input        => Game.Input;
    public static Renderer     Renderer     => Game.Renderer;
    public static AssetManager AssetManager => Game.AssetManager;
    public static SceneManager SceneManager => Game.SceneManager;
    public static SaveManager  SaveManager  => Game.SaveManager;
    public static UIManager    UI           => Game.UI;
}