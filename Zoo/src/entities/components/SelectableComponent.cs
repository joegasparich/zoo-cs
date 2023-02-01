namespace Zoo.entities; 

public class SelectableComponent : Component {
    protected override Type[] Dependencies => new[] { typeof(RenderComponent) };

    public SelectableComponent(Entity entity, ComponentData? data) : base(entity, data) {}
}