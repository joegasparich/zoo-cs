using Newtonsoft.Json;
using Zoo.entities;
using Zoo.util;

namespace Zoo.ai;

public enum CompleteState {
    Incomplete,
    Success,
    Failed,
    Interrupted,
    Expired,
}

public abstract class Behaviour : ISerialisable {
    // References
    protected Actor actor;
    
    // State
    public  CompleteState State;
    private int           expireTick = -1;
    private int           stepIndex  = 0;
    private Step?         currentStep;
    
    // Properties
    public PathFollowComponent Pather  => actor.GetComponent<PathFollowComponent>();

    [JsonConstructor]
    public Behaviour() {}
    public Behaviour(Actor actor) {
        this.actor = actor;
    }

    public virtual void Start() {
        State = CompleteState.Incomplete;
    }
    public virtual void OnComplete() {
        Pather.ResetPath();
    }
    public virtual void OnExpire() {
        Pather.ResetPath();
    }
    public virtual void End() {}
    
    /// <summary>
    /// This method returns a list of steps to be executed in order.
    /// Should be entirely deterministic for serialisation purposes
    /// </summary>
    public virtual IEnumerable<Step> GetSteps() => Enumerable.Empty<Step>();

    public virtual void Update() {
        // Update current step
        currentStep?.Update?.Invoke();
        
        // A step completed, move to the next one
        if (currentStep is { Complete: true }) {
            currentStep = null;
            stepIndex++;
        }
        
        // A step failed, end the behaviour
        if (currentStep is { Failed: true }) {
            State = CompleteState.Failed;
            return;
        }
        
        // Expired, end the behaviour
        if (expireTick != -1 && Game.Ticks >= expireTick) {
            State = CompleteState.Expired;
            return;
        }

        // Completed all steps, end the behaviour
        if (stepIndex >= GetSteps().Count()) {
            State = CompleteState.Success;
            return;
        }
        
        // No current step, start new one
        if (currentStep == null) {
            currentStep             = GetSteps().ElementAt(stepIndex);
            currentStep.Actor       = actor;
            currentStep.StartedTick = Game.Ticks;
            currentStep.Setup?.Invoke();
        }
    }
        
    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("actor",      () => actor.Id, id => actor = Game.GetEntityById(id) as Actor);
        Find.SaveManager.ArchiveValue("state",      ref State);
        Find.SaveManager.ArchiveValue("expireTick", ref expireTick);
        Find.SaveManager.ArchiveValue("stepIndex",  ref stepIndex);
    }
}