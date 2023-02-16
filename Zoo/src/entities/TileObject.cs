using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.entities; 

public class TileObject : Entity {
    // State
    private Side rotation;

    // Properties
    public override ObjectDef       Def         => (ObjectDef)base.Def;
    private         RenderComponent Renderer    => GetComponent<RenderComponent>()!;
    public new      Vector2         Pos         => base.Pos;

    public TileObject(Vector2 pos, ObjectDef def) : base(pos, def) {}

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        Find.World.RegisterTileObject(this);

        if (Def.Solid) {
            foreach (var tile in GetOccupiedTiles()) {
                Find.World.UpdateAccessibilityGrids(tile);
            }

            Messenger.Fire(EventType.PlaceSolid, GetOccupiedTiles().ToList());
        }
    }

    public override void Destroy() {
        if (Def.Solid) {
            foreach (var tile in GetOccupiedTiles()) {
                Find.World.UpdateAccessibilityGrids(tile);
            }
        }
        
        Find.World.UnregisterTileObject(this);
        
        base.Destroy();
    }
    
    public void SetRotation(Side rotation) {
        if (!Def.CanRotate) return;

        this.rotation = rotation;
        Renderer.SpriteIndex = (int)rotation;
    }

    public override IEnumerable<IntVec2> GetOccupiedTiles() {
        var baseTile = (Pos - Def.Size / 2).Floor();
        for (var i = 0; i < Def.Size.X; i++) {
            for (var j = 0; j < Def.Size.Y; j++) {
                yield return baseTile + new IntVec2(i, j);
            }
        }
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("rotation",    ref rotation);
    }

    public override List<InfoTab> GetInfoTabs() {
        var tabs = new List<InfoTab>();
        tabs.Add(new InfoTab(Def.Name, rect => {
            var listing = new Listing(rect);
            listing.Header(Def.Name.Capitalise());
            listing.Label($"Type: {Def.Type.ToString().Capitalise()}");
            listing.Label($"Solid: {Def.Solid.ToString().Capitalise()}");
        }));
        tabs.AddRange(base.GetInfoTabs());
        return tabs;
    }
}