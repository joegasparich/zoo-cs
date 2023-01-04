﻿using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class TileObjectComponent : Component {
    public  ObjectData Data;
    private Side       rotation;
    
    // Component refs
    private RenderComponent renderer;
    
    public TileObjectComponent(Entity entity) : base(entity) {
        renderer = entity.GetComponent<RenderComponent>();
    }

    public override void Start() {
        base.Start();
        
        Find.World.RegisterTileObject(entity);
    }
    
    public override void End() {
        base.End();
        
        Find.World.UnregisterTileObject(entity);
    }

    public void SetRotation(Side rotation) {
        if (!Data.CanRotate) return;
        
        this.rotation = rotation;
        renderer.SpriteIndex = (int)rotation;
    }

    public IEnumerable<IntVec2> GetOccupiedTiles() {
        var baseTile = (entity.Pos - Data.Size / 2).Floor();
        for (var i = 0; i < Data.Size.X; i++) {
            for (var j = 0; j < Data.Size.Y; j++) {
                yield return baseTile + new IntVec2(i, j);
            }
        }
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("objectId",
            () => Data.Id,
            path => Data = Find.Registry.GetObject(path)
        );
        Find.SaveManager.ArchiveValue("rotation", ref rotation);

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            renderer.Graphics    = Data.GraphicData;
            renderer.SpriteIndex = (int)rotation;
        }
    }
}