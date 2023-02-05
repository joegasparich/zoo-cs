using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.tools;

public class Tool_Entity : Tool {
    // Virtual Properties
    public override string   Name => "Entity Tool";
    public override ToolType Type => ToolType.Entity;

    // State
    protected EntityDef? currentEntity;

    public Tool_Entity(ToolManager tm) : base(tm) {}

    public override void Set() {
        Ghost.Type    = GhostType.Sprite;
        Ghost.Elevate = true;
    }

    public override void OnInput(InputEvent evt) {
        if (currentEntity == null) return;

        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            if (!Ghost.CanPlace) return;

            var entity = CreateEntity(currentEntity, Ghost.Pos);
            if (entity == null) return;
            Game.RegisterEntity(entity);

            toolManager.PushAction(new ToolAction() {
                Name = $"Place {currentEntity.Name}",
                Data = entity.Id,
                Undo = data => entity.Destroy(),
            });

            evt.Consume();
        }

        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_RIGHT && currentEntity != null) {
            SetEntity(null);
            evt.Consume();
        }
    }

    public virtual Entity CreateEntity(EntityDef def, Vector2 pos) {
        return GenEntity.CreateEntity(def, pos);
    }

    public override bool CanPlace(ToolGhost ghost) {
        if (currentEntity == null) return false;

        var tileObject = Find.World.GetTileObjectAtTile(ghost.Pos.Floor());
        if (tileObject != null && tileObject.Def.Solid) return false;

        return true;
    }

    protected virtual void SetEntity(EntityDef data) {
        currentEntity = data;

        if (currentEntity != null) {
            Ghost.Graphics = data.GraphicData;
            Ghost.Visible  = true;
        } else {
            Ghost.Visible = false;
        }
    }
}