using System.Numerics;
using System.Text.Json.Nodes;
using Zoo.defs;

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

    public static void LoadEntities(JsonArray data) {
        var parent = Find.SaveManager.CurrentSaveNode;
        Find.SaveManager.Mode = SerialiseMode.Loading;
        
        foreach (JsonObject entityData in data) {
            Find.SaveManager.CurrentSaveNode = entityData;
            var type = Type.GetType(Find.SaveManager.Deserialise<string>(entityData["type"]));
            var def  = Find.AssetManager.GetDef(Find.SaveManager.Deserialise<string>(entityData["defId"])) as EntityDef;
            var pos  = Find.SaveManager.Deserialise<Vector2>(entityData["pos"]);

            var entity = GenEntity.CreateEntity(type, pos, def);
            entity.Serialise();
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
            Type componentType = Type.GetType(entityData["type"].ToString());
            if (entity.HasComponent(componentType)) {
                var component = entity.GetComponent(componentType);
                component.Serialise();
            } else {
                var component = (Component)Activator.CreateInstance(componentType, entity);
                component.Serialise();
                entity.AddComponent(component);
            }
        }
        
        Find.SaveManager.CurrentSaveNode = parent;
    }
}