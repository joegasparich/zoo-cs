using System.Numerics;

namespace Zoo.entities; 

public static class GenEntity {
    public static Entity? CreateTileObject(string assetPath, Vector2 pos) {
        if (!Find.World.IsPositionInMap(pos)) return null;

        var data = Find.Registry.GetObject(assetPath);

        var entity = new Entity(pos + data.Size / 2f);

        // Renderer
        var renderer = entity.AddComponent<RenderComponent>();
        renderer.Origin = data.Origin;
        if (data.Sprite.HasValue) {
            renderer.SetSprite(data.SpritePath);
        } else if (data.SpriteSheet.HasValue) {
            renderer.SetSprite(data.SpriteSheet.Value.TexturePath);
            renderer.SetSource(data.SpriteSheet.Value.GetCellBounds(0));
        }
        
        // Tile object
        var tileObject = entity.AddComponent<TileObjectComponent>();
        tileObject.Data = data;
        
        entity.AddComponent<ElevationComponent>();

        return entity;
    }
}