using Zoo.ui;

namespace Zoo.entities;

public abstract class ComponentData {
    public abstract Type CompClass { get; }
}

public abstract class Component : ISerialisable {
    // References
    protected Entity        entity;
    public    ComponentData data;
    
    // Properties
    protected virtual Type[] Dependencies => Array.Empty<Type>();
    
    public Component(Entity entity, ComponentData? data) {
        this.entity = entity;
        this.data   = data;
    }

    public virtual void Start() {
        foreach (var dependency in Dependencies) {
            Debug.Assert(entity.HasComponent(dependency), $"Entity {entity} does not have dependency {dependency}");
        }
    }
    public virtual void     PreUpdate()             {}
    public virtual void     Update()                {}
    public virtual void     PostUpdate()            {}
    public virtual void     Render()                {}
    public virtual void     OnInput(InputEvent evt) {}
    public virtual void     End()                   {}
    public virtual InfoTab? GetInfoTab()            => null;

    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("type", () => GetType().ToString(), null);
    }
}