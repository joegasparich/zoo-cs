using Zoo.util;

namespace Zoo.entities; 

public class TileObjectComponent : Component {
    public ObjectData Data;
    
    public TileObjectComponent(Entity entity) : base(entity) {}

    public override void Start() {
        base.Start();
        
        Find.World.RegisterTileObject(entity);
    }
    
    public override void End() {
        base.End();
        
        Find.World.UnregisterTileObject(entity);
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

        if (Find.SaveManager.Mode == SerialiseMode.Loading)
            entity.GetComponent<RenderComponent>().Graphics = Data.GraphicData;
    }
}