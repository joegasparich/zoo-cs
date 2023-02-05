using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;

namespace Zoo.tools; 

public class Tool_Animal : Tool_Actor {
    // Constants
    private const int ButtonSize = 30;
    
    // References
    private List<AnimalDef> allAnimals;

    // Properties
    public override string   Name         => "Animal Tool";
    public override ToolType Type         => ToolType.Animal;
    private         AnimalDef CurrentAnimal => currentEntity as AnimalDef;

    public Tool_Animal(ToolManager tm) : base(tm) {
        allAnimals = Find.AssetManager.GetAllDefs<AnimalDef>();
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immAnimalPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var animal in allAnimals) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);
                
                if (GUI.ButtonEmpty(buttonRect, selected: CurrentAnimal != null && CurrentAnimal.Id == animal.Id))
                    SetEntity(animal);
                
                // TODO: Write an icon helper for this
                GUI.DrawSubTexture(buttonRect.ContractedBy(2), animal.GraphicData.Value.Texture, animal.GraphicData.Value.GetCellBounds(0));
                
                i++;
            }
        });
    }

    public override Entity CreateEntity(EntityDef def, Vector2 pos) {
        return GenEntity.CreateAnimal(def as AnimalDef, pos);
    }
}