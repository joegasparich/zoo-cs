using System.Numerics;
using Zoo.defs;
using Zoo.util;

namespace Zoo.entities; 

public static class GenEntity {
    public static T? CreateEntity<T>(Vector2 pos, EntityDef def) where T : Entity {
        if (!Find.World.IsPositionInMap(pos)) return null;

        try {
            var entity = Activator.CreateInstance(typeof(T), pos, def) as T;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity of type {typeof(T).Name} at {pos} with def {def.Id}: ", e);
            return null;
        }
    }
    
    public static Entity CreateEntity(Type type, Vector2 pos, EntityDef def) {
        if (!Find.World.IsPositionInMap(pos)) return null;
        
        try {
            var entity = Activator.CreateInstance(type, pos, def) as Entity;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity of type {type.Name} at {pos} with def {def.Id}: ", e);
            return null;
        }
    }
    
    public static TileObject? CreateTileObject(string objectId, Vector2 pos) {
        var def = Find.AssetManager.GetDef<ObjectDef>(objectId);

        return CreateEntity<TileObject>(pos + def.Size / 2f, def);
    }
    
    public static Animal? CreateAnimal(string animalId, Vector2 pos) {
        var def = Find.AssetManager.GetDef<AnimalDef>(animalId);

        return CreateEntity<Animal>(pos, def);
    }

    public static Guest? CreateGuest(Vector2 pos) {
        return CreateEntity<Guest>(pos, ActorDefOf.Guest);
    }
}