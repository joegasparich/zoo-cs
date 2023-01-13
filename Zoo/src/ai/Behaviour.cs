using Zoo.entities;

namespace Zoo.ai; 

public abstract class Behaviour : ISerialisable {
    protected Actor actor;

    public Behaviour() {}
    public Behaviour(Actor actor) {
        this.actor = actor;
    }
    
    public virtual void Start()     { }
    public virtual void End()       { }
    public virtual void Update()    { }
    public virtual void Serialise() { }
}