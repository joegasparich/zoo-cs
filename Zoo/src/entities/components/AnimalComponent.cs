using Zoo.defs;
using Zoo.ui;
using Zoo.util;

namespace Zoo.entities; 

public class AnimalComponent : Component {
    // References
    private RenderComponent renderer;
    
    // Config
    public AnimalDef Def;
    
    // Properties
    protected override Type[] Dependencies => new[] { typeof(RenderComponent) };

    public AnimalComponent(Entity entity) : base(entity) {
        renderer = entity.GetComponent<RenderComponent>();
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("animalId",
            () => Def.Id,
            id => Def = Find.AssetManager.Get<AnimalDef>(id)
        );
        
        if (Find.SaveManager.Mode == SerialiseMode.Loading) {
            renderer.Graphics = Def.GraphicData;
        }
    }

    public override InfoTab GetInfoTab() {
        return new InfoTab(Def.Name, rect => {
            var listing = new Listing(rect);
            listing.Header(Def.Name.Capitalise());
            listing.Label($"Can swim: {Def.CanSwim.ToString().Capitalise()}");
        });
    }
}