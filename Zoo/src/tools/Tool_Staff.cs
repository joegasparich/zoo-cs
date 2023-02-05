using System.Numerics;
using Raylib_cs;
using Zoo.defs;
using Zoo.entities;
using Zoo.ui;
using Zoo.util;

namespace Zoo.tools;

public class Tool_Staff : Tool_Actor {
    // Constants
    private const int ButtonSize = 30;

    // References
    private List<ActorDef> allStaff;

    // Cache
    private Dictionary<ActorDef, Texture2D> iconTextures = new();

    // Properties
    public override string    Name          => "Staff Tool";
    public override ToolType  Type          => ToolType.Staff;

    public Tool_Staff(ToolManager tm) : base(tm) {
        allStaff = Find.AssetManager.GetEntityDefsWithTag<ActorDef>(EntityTag.Staff);
    }

    public override void Set() {
        base.Set();

        foreach (var staff in allStaff) {
            iconTextures.Add(staff, PersonComponent.GetPersonTexture(staff));
        }
    }

    public override void OnGUI() {
        Find.UI.DoImmediateWindow("immStaffPanel", new Rectangle(10, 60, 200, ButtonSize + GUI.GapSmall * 2), inRect => {
            var i = 0;
            foreach (var staff in allStaff) {
                // TODO: Wrap
                var buttonRect = new Rectangle(i * (ButtonSize + GUI.GapSmall) + GUI.GapSmall, GUI.GapSmall, ButtonSize, ButtonSize);

                if (GUI.ButtonEmpty(buttonRect, selected: CurrentActor != null && CurrentActor.Id == staff.Id))
                    SetEntity(staff);

                // TODO: Write an icon helper for this
                GUI.DrawSubTexture(buttonRect.ContractedBy(2), iconTextures[staff], staff.GraphicData.Value.GetCellBounds(0));

                i++;
            }
        });
    }

    public override Entity CreateEntity(EntityDef def, Vector2 pos) {
        return GenEntity.CreateEntity<Staff>(def, pos);
    }

    protected override void SetEntity(EntityDef data) {
        base.SetEntity(data);

        if (data != null) {
            var graphics = data.GraphicData.Value;
            graphics.Texture = iconTextures[data as ActorDef];
            Ghost.Graphics   = graphics;
        }

    }
}