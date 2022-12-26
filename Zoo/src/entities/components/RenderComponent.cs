using System.Numerics;
using Raylib_cs;

namespace Zoo.entities; 

public class RenderComponent : Component {
    private Texture2D sprite;
    private string    spritePath = "";
    private Rectangle source = new (0, 0, 1, 1);
    private Vector2   origin = new(0.5f, 0.5f);
    
    public RenderComponent(Entity entity) : base(entity) {}

    public override void Render() {
        Find.Renderer.Blit(
            sprite,
            entity.Pos,
            Find.Renderer.GetDepth(entity.Pos.Y),
            new Vector2(sprite.width * source.width, sprite.height * source.height) * Renderer.PixelScale,
            origin,
            source
        );
    }

    public void SetSprite(string path) {
        sprite = Find.AssetManager.GetTexture(path);
        spritePath = path;
    }

    public void SetSource(Rectangle source) {
        this.source = source;
    }
}