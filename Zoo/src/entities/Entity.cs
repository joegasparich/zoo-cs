using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.entities; 

public class Entity {
    public int     Id  { get; set; }
    public Vector2 Pos { get; set; }

    private Dictionary<Type, Component> components = new();

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
    
    // TODO: Serialise
}