﻿using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;

namespace Zoo.tools; 

public class Tool_TileObject : Tool {
    private const int ButtonSize = 30;

    private ObjectData       currentObject;
    private List<ObjectData> allObjects;

    public override string   Name => "Object Tool";
    public override ToolType Type => ToolType.TileObject;

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

            var obj = GenEntity.CreateTileObject(currentObject.AssetPath, Ghost.Pos);
            if (obj == null) return;

            Game.RegisterEntity(obj);
            
            toolManager.PushAction(new ToolAction() {
                Name = $"Place {currentObject.Name}",
                Data = obj.Id,
                Undo = data => Game.GetEntityById((int)data).Destroy(),
            });
            
            evt.Consume();
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immObjectPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var obj in allObjects) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);
                
                GUI.DrawRect(buttonRect, GUI.UIButtonColour);
                
                if (obj.Sprite.HasValue)
                    GUI.DrawTexture(buttonRect.ContractedBy(2), obj.Sprite.Value);
                else if (obj.SpriteSheet != null)
                    GUI.DrawSubTexture(buttonRect.ContractedBy(2), obj.SpriteSheet.Texture, obj.SpriteSheet.GetCellBounds(0));
                
                GUI.HighlightMouseover(buttonRect);
                
                if (currentObject.AssetPath == obj.AssetPath)
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

        if (data.Sprite.HasValue) {
            Ghost.Type   = GhostType.Sprite;
            Ghost.Sprite = data.Sprite;
        } else if (data.SpriteSheet != null) {
            Ghost.Type        = GhostType.SpriteSheet;
            Ghost.SpriteSheet = data.SpriteSheet;
            Ghost.SpriteIndex = 0;
        } else {
            Raylib.TraceLog(TraceLogLevel.LOG_WARNING, $"[TileObjectTool] No object sprite: {data.Name}");
        }

        Ghost.Origin = data.Origin;
        Ghost.Offset = data.Size / 2f;
    }
}