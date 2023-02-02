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
    public  GraphicData       BaseGraphic;
    private GraphicData       bakedGraphic;
    private List<GraphicData> attachments = new();

    // Properties
    public RenderComponentData Data    => (RenderComponentData)data;
    public bool                Hovered => entity.Selectable && Find.Renderer.GetPickIdAtPos(Find.Input.GetMousePos()) == entity.Id;

    public RenderComponent(Entity entity, RenderComponentData? data) : base(entity, data) {
        BaseGraphic = data.GraphicData;
    }

    public override void Start() {
        base.Start();
        
        Debug.Assert(!BaseGraphic.Texture.Empty(), "Sprite missing from render component");

        BakeGraphics();
    }

    public override void Render() {
        bakedGraphic.Blit(
            pos: (entity.Pos + Offset) * World.WorldScale,
            depth: Find.Renderer.GetDepth(entity.Pos.Y),
            colour: OverrideColour,
            index: SpriteIndex,
            pickId: entity.Selectable ? entity.Id : null,
            fragShader: Hovered ? Renderer.OutlineShader : null
        );

        OverrideColour = Color.WHITE;
    }

    public void AddAttachment(string spritePath) {
        var attachment = BaseGraphic;
        attachment.SetSprite(spritePath);
        attachments.Add(attachment);

        BakeGraphics();
    }

    private void BakeGraphics()
    {
        // Bake the attachments into the texture
        // Currently assumes that the attachment will have the exact same dimensions as the main sprite
        var renderTexture = Raylib.LoadRenderTexture(BaseGraphic.Texture.width, BaseGraphic.Texture.height);

        Raylib.BeginTextureMode(renderTexture);
        Raylib.ClearBackground(new Color(0, 0, 0, 0));
        Raylib.DrawTexturePro(
            BaseGraphic.Texture,
            new Rectangle(0, 0, BaseGraphic.Texture.width, -BaseGraphic.Texture.height),
            new Rectangle(0, 0, BaseGraphic.Texture.width, BaseGraphic.Texture.height),
            new Vector2(0, 0),
            0,
            Color.WHITE
        );
        foreach (var att in attachments) {
            Raylib.DrawTexturePro(
                att.Texture,
                new Rectangle(0, 0, BaseGraphic.Texture.width, -BaseGraphic.Texture.height),
                new Rectangle(0, 0, BaseGraphic.Texture.width, BaseGraphic.Texture.height),
                new Vector2(0, 0),
                0,
                Color.WHITE
            );
        }
        Raylib.EndTextureMode();

        bakedGraphic.Texture = renderTexture.texture;
    }
}