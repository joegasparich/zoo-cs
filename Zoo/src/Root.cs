using Raylib_cs;

namespace Zoo;

static class Root
{
    public static void Main()
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_ERROR);
            
        Game.Run();
    }
}