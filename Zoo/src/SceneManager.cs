using Raylib_cs;

namespace Zoo; 

public class SceneManager {
    private Scene currentScene;
    
    // TODO: Add progress callback
    public void LoadScene(Scene scene) {
        if (currentScene != null) {
            Debug.Log($"Stopping scene: {currentScene.Name}");
            currentScene.Stop();
        }
        
        currentScene = scene;
        currentScene.Start();
    }
    
    public Scene GetCurrentScene() {
        return currentScene;
    }
}