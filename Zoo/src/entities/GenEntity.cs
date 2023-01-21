using System.Numerics;
using Zoo.defs;
using Zoo.util;

namespace Zoo.entities; 

public static class GenEntity {
    public static T? CreateEntity<T>(Vector2 pos, EntityDef def) where T : Entity {
        if (!Find.World.IsPositionInMap(pos)) return null;
        
        var entity = Activator.CreateInstance(typeof(T), pos, def) as T;

        foreach (var compData in def.Components) {
            entity.AddComponent(compData.CompClass, compData);
        }

        return entity;
    }
    
    public static Entity CreateEntity(Type type, Vector2 pos, EntityDef def) {
        if (!Find.World.IsPositionInMap(pos)) return null;
        
        var entity = Activator.CreateInstance(type, pos, def) as Entity;

        foreach (var compData in def.Components) {
            entity.AddComponent(compData.CompClass, compData);
        }

        return entity;
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
        var def = Find.AssetManager.GetDef<ActorDef>("Guest");

        return CreateEntity<Guest>(pos, def);
    }
}