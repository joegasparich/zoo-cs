using Zoo.entities;
using Zoo.util;

namespace Zoo.ai;

public class Step {
    // References
    public Actor Actor;
    
    // Config
    public Action?          Setup;
    public Action?          Update;
    public List<Func<bool>> SuccessConditions = new();
    public List<Func<bool>> FailConditions    = new();

    // State
    public int StartedTick;
    
    // Properties
    public bool Complete => SuccessConditions.NullOrEmpty() || SuccessConditions.Any(c => c());
    public bool Failed   => FailConditions.Any(c => c());
    
    public Step SucceedOn(Func<bool> condition) {
        SuccessConditions.Add(condition);

        return this; // For chaining
    }

    public Step FailOn(Func<bool> condition) {
        FailConditions.Add(condition);

        return this; // For chaining
    }
}