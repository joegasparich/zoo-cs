using System.Numerics;
using Raylib_cs;
using Zoo.ui;

namespace Zoo.entities; 

public class Entity : ISerialisable {
    // Config
    public  int                         Id;
    private Dictionary<Type, Component> components = new();
    
    // State
    public Vector2 Pos;
    private string infoDialogId;
    
    // Properties
    public IEnumerable<Component> Components => components.Values;

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

    public void OnInput(InputEvent evt) {
        foreach (var component in components.Values) {
            component.OnInput(evt);

            if (evt.consumed) return;
        }
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT && Find.Renderer.GetPickIdAtPos(evt.mousePos) == Id) {
            if (!Find.UI.IsWindowOpen(infoDialogId)) {
                infoDialogId = Find.UI.PushWindow(new Dialog_Info(this));
            }
                
            evt.Consume();
        }
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
        if (!HasComponent(typeof(T))) return null;
        
        if (components.ContainsKey(typeof(T)))
            return (T)components[typeof(T)];
        
        // TODO (optimisation): Is there a faster way to do this?
        foreach (var type in components.Keys) {
            if (type.IsSubclassOf(typeof(T))) {
                return (T)components[type];
            }
        }
        
        return null;
    }

    public bool HasComponent(Type type) {
        if (components.ContainsKey(type)) return true;
        
        // TODO (optimisation): Is there a faster way to do this?
        foreach (var t in components.Keys) {
            if (t.IsSubclassOf(type)) {
                return true;
            }
        }

        return false;
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