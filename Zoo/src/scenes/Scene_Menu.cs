using Zoo.ui;

namespace Zoo; 

public class Scene_Menu : Scene {
    // Constants
    private const string SceneName = "Menu";
    
    // State
    private string windowHandle;
    
    public Scene_Menu() : base(SceneName) {}

    public override void Start() {
        base.Start();
        
        windowHandle = Find.UI.PushWindow(new Window_Menu());
    }

    public override void Stop() {
        base.Stop();
        
        Find.UI.CloseWindow(windowHandle);
    }
}