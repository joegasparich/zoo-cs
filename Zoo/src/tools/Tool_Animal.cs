using Raylib_cs;
using Zoo.defs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;

namespace Zoo.tools; 

public class Tool_Animal : Tool {
    // Constants
    private const int ButtonSize = 30;
    
    // References
    private List<AnimalDef> allAnimals;

    // Virtual Properties
    public override string   Name => "Animal Tool";
    public override ToolType Type => ToolType.Animal;

    // State
    private AnimalDef? currentAnimal;

    public Tool_Animal(ToolManager tm) : base(tm) {
        allAnimals = Find.AssetManager.GetAllDefs<AnimalDef>();
    }

    public override void Set() {
        Ghost.Type    = GhostType.Sprite;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        if (currentAnimal == null) return;
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!Ghost.CanPlace) return;

            var animal = GenEntity.CreateAnimal(currentAnimal.Id, Ghost.Pos);
            if (animal == null) return;
            Game.RegisterEntity(animal);
            
            toolManager.PushAction(new ToolAction() {
                Name = $"Place {currentAnimal.Name}",
                Data = animal.Id,
                Undo = data => animal.Destroy(),
            });
            
            evt.Consume();
        }
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT && currentAnimal != null) {
            SetAnimal(null);
            evt.Consume();
        }

    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immAnimalPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var animal in allAnimals) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);
                
                if (GUI.ButtonEmpty(buttonRect, selected: currentAnimal != null && currentAnimal.Id == animal.Id))
                    SetAnimal(animal);
                
                // TODO: Write an icon helper for this
                GUI.DrawSubTexture(buttonRect.ContractedBy(2), animal.GraphicData.Value.Texture, animal.GraphicData.Value.GetCellBounds(0));
                
                i++;
            }
        });
    }

    public override bool CanPlace(ToolGhost ghost) {
        if (currentAnimal == null) return false;

        var tileObject = Find.World.GetTileObjectAtTile(ghost.Pos.Floor());
        if (tileObject != null && tileObject.Def.Solid) return false;

        if (!currentAnimal.CanSwim && Find.World.Elevation.IsPositionWater(ghost.Pos.Floor())) return false;

        return true;
    }

    private void SetAnimal(AnimalDef data) {
        currentAnimal = data;

        if (currentAnimal != null) {
            Ghost.Graphics = data.GraphicData;
            Ghost.Visible  = true;
        } else {
            Ghost.Visible = false;
        }
    }
}