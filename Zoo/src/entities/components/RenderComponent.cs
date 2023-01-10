using System.Numerics;
using Raylib_cs;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities;

public class RenderComponentData : ComponentData {
    public override Type CompClass => typeof(RenderComponent);

    public GraphicData GraphicData;
}

public class RenderComponent : Component {
    // State
    public Vector2 Offset         = Vector2.Zero;
    public Color   OverrideColour = Color.WHITE;
    public int     SpriteIndex    = 0;
    
    // Properties
    public RenderComponentData Data => (RenderComponentData)data;
    public GraphicData Graphics => Data.GraphicData;

    public RenderComponent(Entity entity, RenderComponentData? data) : base(entity, data) {}

    public override void Start() {
        base.Start();
        
        Debug.Assert(!Graphics.Sprite.Empty(), "Sprite missing from render component");
    }

    public override void Render() {
        Graphics.Blit(
            pos: (entity.Pos + Offset) * World.WorldScale,
            depth: Find.Renderer.GetDepth(entity.Pos.Y),
            colour: OverrideColour,
            index: SpriteIndex,
            pickId: entity.Id
        );
        
        OverrideColour = Color.WHITE;
    }
}