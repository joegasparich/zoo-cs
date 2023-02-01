using Raylib_cs;
using Zoo.entities;
using Zoo.ui;

namespace Zoo; 

public class SelectionManager {
    // State
    public Entity SelectedEntity;
    private string infoDialogId;

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

    public void Select(Entity? entity) {
        if (Find.UI.IsWindowOpen(infoDialogId))
            Find.UI.CloseWindow(infoDialogId);
        
        SelectedEntity = entity;
        
        if (entity != null)
            infoDialogId = Find.UI.PushWindow(new Dialog_EntityInfo(entity));
    }
}