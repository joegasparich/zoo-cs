using System.Numerics;
using Raylib_cs;
using Zoo.world;

namespace Zoo.entities; 

public class RenderComponent : Component {
    private Texture2D sprite;
    private string    spritePath = "";
    private Rectangle source = new (0, 0, 1, 1);
    
    public Vector2 Origin { get; set; } = new (0.5f, 0.5f);
    public Vector2 Offset { get; set; } = Vector2.Zero;
    
    public RenderComponent(Entity entity) : base(entity) {}

    public override void Render() {
        Find.Renderer.Blit(
            texture: sprite,
            pos: (entity.Pos + Offset) * World.WorldScale,
            depth: Find.Renderer.GetDepth(entity.Pos.Y),
            scale: new Vector2(sprite.width * source.width, sprite.height * source.height) * Renderer.PixelScale,
            origin: Origin,
            source: source
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