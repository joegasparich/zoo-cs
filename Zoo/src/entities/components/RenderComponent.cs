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
    public  Vector2           Offset         = Vector2.Zero;
    public  Color             OverrideColour = Color.WHITE;
    public  int               SpriteIndex    = 0; // TODO: move this shit into graphics now that they are per instance
    public  GraphicData       Graphics;
    private List<GraphicData> attachments = new();
    
    // Properties
    public RenderComponentData Data => (RenderComponentData)data;

    public RenderComponent(Entity entity, RenderComponentData? data) : base(entity, data) {
        Graphics = data.GraphicData;
    }

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

        foreach (var attachment in attachments) {
            attachment.Blit(
                pos: (entity.Pos + Offset) * World.WorldScale,
                depth: Find.Renderer.GetDepth(entity.Pos.Y),
                colour: OverrideColour,
                pickId: entity.Id
            );
        }
        
        OverrideColour = Color.WHITE;
    }

    public void AddAttachment(string spritePath) {
        var attachment = Graphics;
        attachment.SpritePath = spritePath;
        attachments.Add(attachment);
    }

    public void AddAttachment(GraphicData graphic) {
        attachments.Add(graphic);
    }
}