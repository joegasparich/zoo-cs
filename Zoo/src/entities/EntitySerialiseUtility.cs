using System.Numerics;
using Newtonsoft.Json.Linq;
using Zoo.defs;

namespace Zoo.entities; 

public static class EntitySerialiseUtility {
    public static JToken SaveEntities(IEnumerable<Entity> entities) {
        var parent = Find.SaveManager.CurrentSaveNode;

        var saveData = new JArray();

        foreach (var entity in entities) {
            var entityData = new JObject();
            Find.SaveManager.CurrentSaveNode = entityData;
            entity.Serialise();
            saveData.Add(entityData);
        }

        Find.SaveManager.CurrentSaveNode = parent;
        return saveData;
    }

    public static void LoadEntities(JArray data) {
        var parent = Find.SaveManager.CurrentSaveNode;
        Find.SaveManager.Mode = SerialiseMode.Loading;
        
        foreach (JObject entityData in data) {
            Find.SaveManager.CurrentSaveNode = entityData;
            var type = Type.GetType(entityData["type"].Value<string>());
            var def  = Find.AssetManager.GetDef(entityData["def"].Value<string>()) as EntityDef;
            var pos  = entityData["pos"].ToObject<Vector2>();

            var entity = GenEntity.CreateEntity(type, def, pos);
            Game.RegisterEntityNow(entity, entityData["id"].Value<int>());
            entity.Serialise();
            entity.Setup(true);
        }
        
        Find.SaveManager.CurrentSaveNode = parent;
    }

    public static JToken SaveComponents(IEnumerable<Component> components) {
        var parent = Find.SaveManager.CurrentSaveNode;

        var saveData = new JArray();

        foreach (var component in components) {
            var componentData = new JObject();
            Find.SaveManager.CurrentSaveNode = componentData;
            component.Serialise();
            saveData.Add(componentData);
        }

        Find.SaveManager.CurrentSaveNode = parent;
        return saveData;
    }
    
    public static void LoadComponents(Entity entity, JToken data) {
        var parent = Find.SaveManager.CurrentSaveNode;

        foreach (var entityData in data as JArray) {
            Find.SaveManager.CurrentSaveNode = entityData as JObject;
            Type componentType = Type.GetType(entityData["type"].Value<string>());
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