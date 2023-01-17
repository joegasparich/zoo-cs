using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities; 

public enum EntityTag {
    All,
    Animal,
    TileObject,
    Consumable
}

public class Entity : ISerialisable {
    // Config
    public  int                         Id;
    private Dictionary<Type, Component> components = new();
    private EntityDef                   def;
    public  HashSet<EntityTag>         Tags { get; } = new();

    // State
    public  Vector2 Pos;
    private string  infoDialogId;
    public  bool    Despawned;
    
    // Properties
    public         IEnumerable<Component> Components => components.Values;
    public virtual EntityDef              Def        => def;

    public Entity(Vector2 pos, EntityDef? def) {
        Pos      = pos;
        this.def = def;

        foreach (var tag in def.Tags) {
            Tags.Add(tag);
        }
        
        Game.RegisterEntity(this);
    }

    public virtual void Setup() {
        if (def.IsStatic)
            Find.World.OccupyTileStatic(this);
        else
            Find.World.OccupyTileDynamic(this);
        
        foreach (var component in components.Values) {
            component.Start();
        }
    }

    public virtual void PreUpdate() {
        if (!def.IsStatic)
            Find.World.OccupyTileDynamic(this);
        
        foreach (var component in components.Values) {
            component.PreUpdate();
        }
    }

    public virtual void Update() {
        foreach (var component in components.Values) {
            component.Update();
        }
    }

    public virtual void PostUpdate() {
        foreach (var component in components.Values) {
            component.PostUpdate();
        }
    }

    public virtual void Render() {
        foreach (var component in components.Values) {
            component.Render();
        }
        
        // Debug rendering
        if (DebugSettings.EntityLocations) {
            Debug.DrawLine(Pos - new Vector2(0.25f, 0.25f), Pos + new Vector2(0.25f, 0.25f), new Color(255, 0, 0, 255), true);
            Debug.DrawLine(Pos - new Vector2(-0.25f, 0.25f), Pos + new Vector2(-0.25f, 0.25f), new Color(255, 0, 0, 255), true);
        }
    }

    public virtual void Destroy() {
        foreach (var component in components.Values) {
            component.End();
        }
        
        if (def.IsStatic)
            Find.World.UnoccupyTileStatic(this);

        Despawned = true;
        
        Game.UnregisterEntity(this);
    }

    public virtual void OnInput(InputEvent evt) {
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
    
    // Add existing component
    public T AddComponent<T>(T component) where T : Component {
        components.Add(component.GetType(), component);
        return component;
    }
    // Generate component from type
    public T AddComponent<T>(ComponentData? data = null) where T : Component {
        var component = (T)Activator.CreateInstance(typeof(T), this, data)!;
        components.Add(component.GetType(), component);
        return component;
    }
    public Component AddComponent(Type type, ComponentData? data = null) {
        var component = (Component)Activator.CreateInstance(type, this, data)!;
        components.Add(component.GetType(), component);
        return component;
    }
    
    public T? GetComponent<T>() where T : Component {
        if (!HasComponent(typeof(T))) return null;
        
        if (components.ContainsKey(typeof(T)))
            return (T)components[typeof(T)];
        
        foreach (var type in components.Keys) {
            if (typeof(T).IsAssignableFrom(type)) {
                return (T)components[type];
            }
        }
        
        return null;
    }
    public Component? GetComponent(Type type) {
        if (!HasComponent(type)) return null;
        
        if (components.ContainsKey(type))
            return components[type];
        
        foreach (var t in components.Keys) {
            if (type.IsAssignableFrom(t)) {
                return components[t];
            }
        }
        
        return null;
    }

    public bool HasComponent<T>() where T : Component {
        if (components.ContainsKey(typeof(T))) return true;
        
        foreach (var t in components.Keys) {
            if (typeof(T).IsAssignableFrom(t)) {
                return true;
            }
        }

        return false;
    }
    
    public bool HasComponent(Type type) {
        if (components.ContainsKey(type)) return true;
        
        foreach (var t in components.Keys) {
            if (type.IsAssignableFrom(t)) {
                return true;
            }
        }

        return false;
    }

    public virtual IEnumerable<IntVec2> GetOccupiedTiles() {
        yield return Pos.Floor();
    }

    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("type", () => GetType().ToString(), null);
        Find.SaveManager.ArchiveValue("id",   ref Id);
        Find.SaveManager.ArchiveValue("pos",  ref Pos);
        
        Find.SaveManager.ArchiveCustom("components",
            () => EntitySerialiseUtility.SaveComponents(components.Values),
            data => EntitySerialiseUtility.LoadComponents(this, data)
        );
        
        Find.SaveManager.ArchiveValue("defId",
            () => def.Id,
            id => def = Find.AssetManager.GetDef(id) as EntityDef
        );
    }

    public virtual List<InfoTab> GetInfoTabs() {
        return Components.Select(comp => comp.GetInfoTab()).Where(tab => tab != null).ToList();
    }
}