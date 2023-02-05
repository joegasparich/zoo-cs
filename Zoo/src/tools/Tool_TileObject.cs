using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_TileObject : Tool {
    // Constants
    private const int ButtonSize = 30;
    
    // References
    private List<ObjectDef> allObjects;

    // Virtual Properties
    public override string   Name => "Object Tool";
    public override ToolType Type => ToolType.TileObject;

    // State
    private ObjectDef? currentObject;
    private Side        rotation;

    public Tool_TileObject(ToolManager tm) : base(tm) {
        allObjects = Find.AssetManager.GetAllDefs<ObjectDef>();
    }

    public override void Set() {
        Ghost.Type    = GhostType.Sprite;
        Ghost.Snap    = true;
        Ghost.Offset  = new Vector2(0.5f, 0.5f);
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        if (currentObject == null) return;
        
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!Ghost.CanPlace) return;

            var obj = GenEntity.CreateTileObject(currentObject.Id, Ghost.Pos);
            if (obj == null) return;

            obj.SetRotation(rotation);
            if (currentObject.NeedsBlueprint)
                obj.IsBlueprint = true;

            Game.RegisterEntity(obj);

            Find.Zoo.DeductFunds(currentObject.Cost);

            toolManager.PushAction(new ToolAction {
                Name = $"Place {currentObject.Name}",
                Data = obj.Id,
                Undo = _ => {
                    Find.Zoo.AddFunds(obj.Def.Cost);
                    obj.Destroy();
                }
            });
            
            evt.Consume();
        }

        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT && currentObject != null) {
            SetObject(null);
            evt.Consume();
            return;
        }

        if (currentObject.CanRotate) {
            // TODO: make sure these are rotating the right way
            if (evt.inputDown == InputType.RotateCounterClockwise) {
                rotation = (Side)JMath.PositiveMod((int)rotation - 1, 4);
                Ghost.SpriteIndex = (int)rotation;
                evt.Consume();
            }

            if (evt.inputDown == InputType.RotateClockwise) {
                rotation = (Side)JMath.PositiveMod((int)rotation + 1, 4);
                Ghost.SpriteIndex = (int)rotation;
                evt.Consume();
            }
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immObjectPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var obj in allObjects) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);
                
                if (GUI.ButtonEmpty(buttonRect, selected: currentObject != null && currentObject.Id == obj.Id))
                    SetObject(obj);
                
                // TODO: Write an icon helper for this
                GUI.DrawSubTexture(buttonRect.ContractedBy(2), obj.GraphicData.Value.Texture, obj.GraphicData.Value.GetCellBounds(0));

                i++;
            }
        });
    }

    public override bool CanPlace(ToolGhost ghost) {
        if (currentObject == null) return false;

        // Can't afford
        if (currentObject.Cost > Find.Zoo.Funds) return false;

        return TileUtility.CanPlace(currentObject, ghost.Pos.Floor());
    }

    private Rectangle GetPlacementBounds(ToolGhost ghost) {
        return new Rectangle(ghost.Pos.X, ghost.Pos.Y, currentObject.Size.X, currentObject.Size.Y);
    }

    private void SetObject(ObjectDef data) {
        currentObject = data;

        if (currentObject != null) {
            Ghost.Graphics = data.GraphicData;
            Ghost.Offset   = data.Size / 2f;
            Ghost.Visible  = true;
        } else {
            Ghost.Visible = false;
        }
    }
}