namespace Zoo.entities; 

public abstract class Component : ISerialisable {
    protected Entity entity;
    
    public Component(Entity entity) {
        this.entity = entity;
    }
    
    public virtual void Start() {}
    public virtual void PreUpdate() {}
    public virtual void Update() {}
    public virtual void PostUpdate() {}
    public virtual void Render() {}
    public virtual void End() {}

    public virtual void Serialise() {
        
    }
}