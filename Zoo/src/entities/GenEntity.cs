using System.Numerics;

namespace Zoo.entities; 

public static class GenEntity {
    public static Entity? CreateTileObject(string objectId, Vector2 pos) {
        if (!Find.World.IsPositionInMap(pos)) return null;

        var data = Find.Registry.GetObject(objectId);

        var entity = new Entity(pos + data.Size / 2f);

        // Renderer
        var renderer = entity.AddComponent<RenderComponent>();
        renderer.Graphics = data.GraphicData;
        
        // Tile object
        var tileObject = entity.AddComponent<TileObjectComponent>();
        tileObject.Data = data;
        
        entity.AddComponent<ElevationComponent>();

        return entity;
    }
}