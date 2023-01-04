using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities;

public class RenderComponent : Component {
    public GraphicData Graphics;
    public Vector2     Offset         = Vector2.Zero;
    public Color       OverrideColour = Color.WHITE;

    public RenderComponent(Entity entity) : base(entity) {
        Graphics = new GraphicData();
        Graphics.Origin = new Vector2(0.5f, 0.5f);
    }

    public override void Start() {
        if (Graphics.Sprite.Empty()) {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, $"Entity {entity.Id} graphic data hasn't been set up");
        }
    }

    public override void Render() {
        Graphics.Blit(
            pos: (entity.Pos + Offset) * World.WorldScale,
            depth: Find.Renderer.GetDepth(entity.Pos.Y),
            colour: OverrideColour
        );
        
        OverrideColour = Color.WHITE;
    }
}