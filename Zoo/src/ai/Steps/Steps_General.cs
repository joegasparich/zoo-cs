using System.Numerics;
using Zoo.world;

namespace Zoo.ai; 

public static class Steps_General {
    public static Step Do(Action action) {
        var step = new Step();
        step.Setup = action;

        return step;
    }
    
    public static Step Wait(int ticks) {
        var step = new Step();
        step.SuccessConditions.Add(() => Game.Ticks - step.StartedTick >= ticks);

        return step;
    }
    
    public static Step GoTo(Vector2 pos, PathMode mode = PathMode.OnTile) {
        var step = new Step();
        step.Setup = () => step.Actor.Pather.PathTo(pos, mode);
        step.SuccessConditions.Add(() => step.Actor.Pather.ReachedDestination);
        step.FailConditions.Add(() => step.Actor.Pather.NoPathFound);

        return step;
    }
}