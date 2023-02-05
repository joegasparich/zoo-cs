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
    private bool isBlueprint;
    
    // Properties
    public override ObjectDef       Def         => (ObjectDef)base.Def;
    private         RenderComponent Renderer    => GetComponent<RenderComponent>()!;
    public          bool            IsBlueprint => isBlueprint;

    public TileObject(Vector2 pos, ObjectDef def) : base(pos, def) {}

    public override void Setup() {
        base.Setup();
        
        Find.World.RegisterTileObject(this);

        if (Def.NeedsBlueprint) {
            isBlueprint              = true;
            Renderer.Graphics.Colour = Color.WHITE.WithAlpha(0.5f);
            Find.Zoo.ObjBlueprints.Add(this);
        }

        // Currently this means that blueprints are solid and can't be walked through
        // Probably a good idea in case they get built while an actor is pathing through it
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

    public void BuildBlueprint() {
        isBlueprint              = false;
        Renderer.Graphics.Colour = Color.WHITE;
        Find.Zoo.ObjBlueprints.Remove(this);
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
        Find.SaveManager.ArchiveValue("isBlueprint", ref isBlueprint);

        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            Renderer.SpriteIndex   = (int)rotation;
        }
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