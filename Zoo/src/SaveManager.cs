using Raylib_cs;

namespace Zoo; 

public class SaveManager {
    public void NewGame() {
        Raylib.TraceLog(TraceLogLevel.LOG_TRACE, "Starting new game");
        Find.SceneManager.LoadScene(new ZooScene());
    }
}