namespace Zoo.entities; 

public abstract class Component {
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
    
    // TODO: Serialise
}