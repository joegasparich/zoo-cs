using Zoo.entities;

namespace Zoo.ai; 

public abstract class Behaviour : ISerialisable {
    protected Entity entity;

    public Behaviour() {}
    public Behaviour(Entity entity) {
        this.entity = entity;
    }
    
    public virtual void Start()     { }
    public virtual void End()       { }
    public virtual void Update()    { }
    public virtual void Serialise() { }
}