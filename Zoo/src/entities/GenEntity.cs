using System.Numerics;
using Zoo.defs;
using Zoo.util;

namespace Zoo.entities; 

public static class GenEntity {
    public static Entity CreateEntity(EntityDef def, Vector2 pos) {
        if (!Find.World.IsPositionInMap(pos)) return null;

        try {
            var entity = Activator.CreateInstance(typeof(Entity), pos, def) as Entity;

            foreach (var compData in def.Components) {
                entity.AddComponent(compData.CompClass, compData);
            }

            return entity;
        } catch (Exception e) {
            Debug.Error($"Failed to create entity at {pos} with def {def.Id}: ", e);
            return null;
        }

    }

    public static T? CreateEntity<T>(EntityDef def, Vector2 pos) where T : Entity {
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
    
    public static Entity CreateEntity(Type type, EntityDef def, Vector2 pos) {
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

        return CreateTileObject(def, pos);
    }
    
    public static TileObject? CreateTileObject(ObjectDef def, Vector2 pos) {
        return CreateEntity<TileObject>(def, pos + def.Size / 2f);
    }
    
    public static Animal? CreateAnimal(string animalId, Vector2 pos) {
        var def = Find.AssetManager.GetDef<AnimalDef>(animalId);

        return CreateAnimal(def, pos);
    }
    
    public static Animal? CreateAnimal(AnimalDef def, Vector2 pos) {
        return CreateEntity<Animal>(def, pos);
    }

    public static Guest? CreateGuest(Vector2 pos) {
        return CreateEntity<Guest>(ActorDefOf.Guest, pos);
    }
}