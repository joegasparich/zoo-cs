﻿namespace Zoo.entities; 

public class ElevationComponent : Component {
    private RenderComponent renderer;

    protected override Type[] Dependencies => new Type[] { typeof(RenderComponent) };

    public ElevationComponent(Entity entity) : base(entity) {}

    public override void Start() {
        base.Start();
        
        renderer = entity.GetComponent<RenderComponent>();
    }

    public override void Update() {
        renderer.Offset = renderer.Offset with{ Y = -Find.World.Elevation.GetElevationAtPos(entity.Pos) };
    }
}