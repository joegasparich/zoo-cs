using Raylib_cs;

namespace Zoo; 

public class SaveManager {
    public void NewGame() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Starting new game");
        Game.SceneManager.LoadScene(new ZooScene());
    }
}