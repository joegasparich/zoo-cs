using System.Numerics;
using Raylib_cs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;
using Zoo.world;

namespace Zoo; 

public class SelectionManager {
    // Constants
    private static readonly CachedTexture SelectionBracketCorner = new("assets/textures/ui/selection_bracket_corner.png");
    private const           int           offset                 = 2;

    // State
    public  Entity? SelectedEntity;
    private string  infoDialogId;

    public void Update() {
        if (SelectedEntity is { Despawned: true })
            Select(null);
    }

    public void OnInput(InputEvent evt) {
        if (evt.mouseDown == MouseButton.MOUSE_BUTTON_LEFT) {
            var pickId = Find.Renderer.GetPickIdAtPos(evt.mousePos);
            if (pickId > 0) {
                Select(Game.GetEntityById(pickId));
            }
            else {
                Select(null);
            }
            evt.Consume();
        }
    }

    public void Render() {
        if (SelectedEntity != null) {
            var (width, height) = SelectedEntity.Graphics.Texture.Dimensions();
            Find.Renderer.Blit(SelectionBracketCorner.Texture, SelectedEntity.Pos * World.WorldScale + new Vector2(-width * SelectedEntity.Graphics.Origin.X       - offset, -height * SelectedEntity.Graphics.Origin.Y       - offset), (int)Depth.UI);
            Find.Renderer.Blit(SelectionBracketCorner.Texture, SelectedEntity.Pos * World.WorldScale + new Vector2(width  * (1 - SelectedEntity.Graphics.Origin.X) + offset, -height * SelectedEntity.Graphics.Origin.Y       - offset), (int)Depth.UI, rotation: 90);
            Find.Renderer.Blit(SelectionBracketCorner.Texture, SelectedEntity.Pos * World.WorldScale + new Vector2(width  * (1 - SelectedEntity.Graphics.Origin.X) + offset, height  * (1 - SelectedEntity.Graphics.Origin.Y) + offset), (int)Depth.UI, rotation: 180);
            Find.Renderer.Blit(SelectionBracketCorner.Texture, SelectedEntity.Pos * World.WorldScale + new Vector2(-width * SelectedEntity.Graphics.Origin.X       - offset, height  * (1 - SelectedEntity.Graphics.Origin.Y) + offset), (int)Depth.UI, rotation: 270);
        }
    }

    public void Select(Entity? entity) {
        if (Find.UI.IsWindowOpen(infoDialogId))
            Find.UI.CloseWindow(infoDialogId);
        
        SelectedEntity = entity;
        
        if (entity != null)
            infoDialogId = Find.UI.PushWindow(new Dialog_EntityInfo(entity));
    }
}