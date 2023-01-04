using System.Numerics;
using Raylib_cs;

namespace Zoo.entities; 

public class Entity : ISerialisable {
    public int     Id;
    public Vector2 Pos;

    private Dictionary<Type, Component> components = new();

    public Entity() {}
    public Entity(Vector2 pos) {
        Pos = pos;
    }

    public void Setup() {
        foreach (var component in components.Values) {
            component.Start();
        }
    }

    public void PreUpdate() {
        foreach (var component in components.Values) {
            component.PreUpdate();
        }
    }

    public void Update() {
        foreach (var component in components.Values) {
            component.Update();
        }
    }

    public void PostUpdate() {
        foreach (var component in components.Values) {
            component.PostUpdate();
        }
    }

    public void Render() {
        foreach (var component in components.Values) {
            component.Render();
        }
        
        // Debug rendering
        // TODO: Toggleable
        Debug.DrawLine(Pos - new Vector2(0.25f, 0.25f), Pos + new Vector2(0.25f, 0.25f), new Color(255, 0, 0, 255), true);
        Debug.DrawLine(Pos - new Vector2(-0.25f, 0.25f), Pos + new Vector2(-0.25f, 0.25f), new Color(255, 0, 0, 255), true);
    }

    public void Destroy() {
        foreach (var component in components.Values) {
            component.End();
        }
        
        Game.UnregisterEntity(this);
    }
    
    public T AddComponent<T>(params object?[]? args) where T : Component {
        args ??= Array.Empty<object>();
        args = args.Prepend(this).ToArray();
        var component = (T)Activator.CreateInstance(typeof(T), args)!;
        components.Add(component.GetType(), component);
        return component;
    }
    public T AddComponent<T>(T component) where T : Component {
        components.Add(component.GetType(), component);
        return component;
    }
    
    public T? GetComponent<T>() where T : Component {
        if (HasComponent(typeof(T))) return (T)components[typeof(T)];
        
        // TODO: Is there a faster way to do this?
        foreach (var type in components.Keys) {
            if (type.IsSubclassOf(typeof(T))) {
                return (T)components[type];
            }
        }
        
        return null;
    }
    public bool HasComponent(Type type) {
        return components.ContainsKey(type);
    }

    public void Serialise() {
        Find.SaveManager.ArchiveValue("id", ref Id);
        Find.SaveManager.ArchiveValue("pos", ref Pos);
        
        Find.SaveManager.ArchiveCustom("components",
            () => EntityUtility.SaveComponents(components.Values),
            data => EntityUtility.LoadComponents(this, data)
        );
    }
}