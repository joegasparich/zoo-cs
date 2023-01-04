using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo.tools; 

public class Tool_TileObject : Tool {
    // Constants
    private const int ButtonSize = 30;
    
    // References
    private List<ObjectData> allObjects;

    // Virtual Properties
    public override string   Name => "Object Tool";
    public override ToolType Type => ToolType.TileObject;

    // State
    private ObjectData       currentObject;
    private Side             rotation;

    public Tool_TileObject(ToolManager tm) : base(tm) {
        allObjects = Find.Registry.GetAllObjects();
    }

    public override void Set() {
        Ghost.Type    = GhostType.Sprite;
        Ghost.Snap    = true;
        Ghost.Offset  = new Vector2(0.5f, 0.5f);
        Ghost.Elevate = true;

        // Temp, should handle having no object
        SetObject(Find.Registry.GetObject(OBJECTS.TREE));
    }

    public override void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!Ghost.CanPlace) return;

            var obj = GenEntity.CreateTileObject(currentObject.Id, Ghost.Pos);
            if (obj == null) return;
            
            obj.GetComponent<TileObjectComponent>().SetRotation(rotation);

            Game.RegisterEntity(obj);
            
            toolManager.PushAction(new ToolAction() {
                Name = $"Place {currentObject.Name}",
                Data = obj.Id,
                Undo = data => Game.GetEntityById((int)data).Destroy(),
            });
            
            evt.Consume();
        }

        if (currentObject.CanRotate) {
            if (evt.keyDown == KeyboardKey.KEY_Q) {
                rotation = (Side)JMath.PositiveMod((int)rotation - 1, 4);
                Ghost.SpriteIndex = (int)rotation;
            }

            if (evt.keyDown == KeyboardKey.KEY_E) {
                rotation = (Side)JMath.PositiveMod((int)rotation + 1, 4);
                Ghost.SpriteIndex = (int)rotation;
            }
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immObjectPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var obj in allObjects) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);
                
                GUI.DrawRect(buttonRect, GUI.UIButtonColour);
                
                // TODO: Write an icon helper for this
                GUI.DrawSubTexture(buttonRect.ContractedBy(2), obj.GraphicData.Sprite, obj.GraphicData.GetCellBounds(0));
                
                GUI.HighlightMouseover(buttonRect);
                
                if (currentObject.Id == obj.Id)
                    GUI.DrawBorder(buttonRect, 2, Color.BLACK);
                
                if (GUI.ClickableArea(buttonRect))
                    SetObject(obj);

                i++;
            }
        });
    }

    public override bool CanPlace(ToolGhost ghost) {
        for (int i = 0; i < currentObject.Size.X; i++) {
            for (int j = 0; j < currentObject.Size.Y; j++) {
                var tile = (ghost.Pos + new Vector2(i, j)).Floor();
                
                if (Find.World.GetTileObjectAtTile(tile) != null) return false;
                if (Find.World.Elevation.IsTileWater(tile)) return false;
                if (!Find.World.IsPositionInMap(tile)) return false;
            }
        }

        return true;
    }

    private void SetObject(ObjectData data) {
        currentObject = data;

        Ghost.Graphics = data.GraphicData;
        Ghost.Offset = data.Size / 2f;
    }
}