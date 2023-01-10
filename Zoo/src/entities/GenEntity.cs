using System.Numerics;
using Zoo.defs;
using Zoo.util;

namespace Zoo.entities; 

public static class GenEntity {
    private static T? CreateEntity<T>(Vector2 pos, EntityDef def) where T : Entity {
        if (!Find.World.IsPositionInMap(pos)) return null;
        
        var entity = Activator.CreateInstance(typeof(T), pos, def) as T;

        foreach (var compData in def.Components) {
            var comp = entity.AddComponent(compData.CompClass);
            comp.data = compData;
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
}