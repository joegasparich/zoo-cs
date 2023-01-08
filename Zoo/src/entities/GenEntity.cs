using System.Numerics;
using Zoo.defs;

namespace Zoo.entities; 

public static class GenEntity {
    public static Entity? CreateAnimal(string animalId, Vector2 pos) {
        if (!Find.World.IsPositionInMap(pos)) return null;

        var def = Find.AssetManager.Get<AnimalDef>(animalId);

        var entity = new Entity(pos);

        // Renderer
        var renderer = entity.AddComponent<RenderComponent>();
        Debug.Assert(def.GraphicData != null);
        renderer.Graphics = def.GraphicData;
        
        // Tile object
        var animal = entity.AddComponent<AnimalComponent>();
        animal.Def = def;
        
        entity.AddComponent<ElevationComponent>();

        return entity;
    }
    
    public static Entity? CreateTileObject(string objectId, Vector2 pos) {
        if (!Find.World.IsPositionInMap(pos)) return null;

        var data = Find.AssetManager.Get<ObjectDef>(objectId);

        var entity = new Entity(pos + data.Size / 2f);

        // Renderer
        var renderer = entity.AddComponent<RenderComponent>();
        Debug.Assert(data.GraphicData != null);
        renderer.Graphics = data.GraphicData;
        
        // Tile object
        var tileObject = entity.AddComponent<TileObjectComponent>();
        tileObject.Data = data;
        
        entity.AddComponent<ElevationComponent>();

        return entity;
    }
}