using System.Numerics;
using Raylib_cs;
using Zoo.ui;

namespace Zoo.entities;

public class SelectableComponent : Component {
    // Properties
    protected override Type[]          Dependencies => new[] { typeof(RenderComponent) };
    private            RenderComponent Renderer     => entity.GetComponent<RenderComponent>();

    public SelectableComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void OnGUI() {
        if (Renderer.Hovered) {
            var textMeasurements = GUI.MeasureText(entity.Name);
            var dimensions = new Vector2(textMeasurements.X + GUI.GapSmall * 2, textMeasurements.Y + GUI.GapSmall * 2);

            Find.UI.DoImmediateWindow(
                $"{entity.Id}-tooltip",
                new Rectangle(
                    Find.Input.GetMousePos().X,
                    Find.Input.GetMousePos().Y - dimensions.Y,
                    dimensions.X,
                    dimensions.Y),
                inRect => {
                    GUI.DrawRect(inRect, Color.DARKBLUE);
                    using (new TextBlock(AlignMode.MiddleCenter, Color.WHITE))
                        GUI.Label(new Rectangle(0, 0, inRect.width, inRect.height), entity.Name);
                });
        }
    }
}