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
        
    }
}