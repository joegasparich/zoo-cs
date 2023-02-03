using Zoo.entities;
using Zoo.util;

namespace Zoo.ai;

public abstract class Behaviour : ISerialisable {
    // References
    protected Actor actor;
    
    // State
    public  bool  Completed;
    private int   expireTick = -1;
    private int   stepIndex  = 0;
    private Step? currentStep;
    
    // Properties
    public bool                Expired => expireTick > 0 && Game.Ticks > expireTick;
    public PathFollowComponent Pather  => actor.GetComponent<PathFollowComponent>();

    public Behaviour() {}
    public Behaviour(Actor actor) {
        this.actor = actor;
    }

    public virtual void Start()      { }
    public virtual void OnComplete() {
        Pather.ResetPath();
    }
    public virtual void OnExpire() {
        Pather.ResetPath();
    }
    
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
            Completed = true;
            return;
        }

        // Completed all steps
        if (stepIndex >= GetSteps().Count()) {
            Completed = true;
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
        Find.SaveManager.ArchiveValue("actor", () => actor.Id, id => actor = Game.GetEntityById(id) as Actor);
        Find.SaveManager.ArchiveValue("completed",  ref Completed);
        Find.SaveManager.ArchiveValue("expireTick", ref expireTick);
        Find.SaveManager.ArchiveValue("stepIndex",  ref stepIndex);
    }
}