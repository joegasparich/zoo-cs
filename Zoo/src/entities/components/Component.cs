namespace Zoo.entities; 

public abstract class Component : ISerialisable {
    // References
    protected Entity entity;
    
    // Properties
    protected virtual Type[] Dependencies => Array.Empty<Type>();
    
    public Component(Entity entity) {
        this.entity = entity;
    }

    public virtual void Start() {
        foreach (var dependency in Dependencies) {
            Debug.Assert(entity.HasComponent(dependency), $"Entity {entity} does not have dependency {dependency}");
        }
    }
    public virtual void PreUpdate() {}
    public virtual void Update() {}
    public virtual void PostUpdate() {}
    public virtual void Render() {}
    public virtual void End() {}

    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("type", () => GetType().ToString(), null);
    }
}