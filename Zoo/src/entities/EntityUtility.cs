using System.Numerics;
using System.Text.Json.Nodes;

namespace Zoo.entities; 

public static class EntityUtility {
    public static JsonNode SaveEntities(IEnumerable<Entity> entities) {
        var parent = Find.SaveManager.CurrentSaveNode;

        var saveData = new JsonArray();

        foreach (var entity in entities) {
            var entityData = new JsonObject();
            Find.SaveManager.CurrentSaveNode = entityData;
            entity.Serialise();
            saveData.Add(entityData);
        }

        Find.SaveManager.CurrentSaveNode = parent;
        return saveData;
    }

    public static void LoadEntities(JsonNode data) {
        var parent = Find.SaveManager.CurrentSaveNode;
        
        foreach (JsonObject entityData in data.AsArray()) {
            Find.SaveManager.CurrentSaveNode = entityData;
            var entity = new Entity(Find.SaveManager.Parse<Vector2>(entityData["pos"]));
            entity.Serialise();
            Game.RegisterEntity(entity, entity.Id);
        }
        
        Find.SaveManager.CurrentSaveNode = parent;
    }

    public static JsonNode SaveComponents(IEnumerable<Component> components) {
        var parent = Find.SaveManager.CurrentSaveNode;

        var saveData = new JsonArray();

        foreach (var component in components) {
            var componentData = new JsonObject();
            Find.SaveManager.CurrentSaveNode = componentData;
            component.Serialise();
            saveData.Add(componentData);
        }

        Find.SaveManager.CurrentSaveNode = parent;
        return saveData;
    }
    
    public static void LoadComponents(Entity entity, JsonNode data) {
        var parent = Find.SaveManager.CurrentSaveNode;

        foreach (JsonObject entityData in data.AsArray()) {
            Find.SaveManager.CurrentSaveNode = entityData;
            Type      componentType = Type.GetType(entityData["type"].ToString());
            Component component     = (Component)Activator.CreateInstance(componentType, entity);
            component.Serialise();
            entity.AddComponent(component);
        }
        
        Find.SaveManager.CurrentSaveNode = parent;
    }
}