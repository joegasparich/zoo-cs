using Raylib_cs;
using Zoo.entities;
using Zoo.util;

namespace Zoo.ui;

public class InfoTab {
    public string            tabName;
    public Action<Rectangle> drawTabContents;
    public InfoTab(string tabName, Action<Rectangle> drawTabContents) {
        this.tabName = tabName;
        this.drawTabContents = drawTabContents;
    }
}

public class Dialog_Info : Dialog {
    // Constants
    private const           int       PanelWidth  = 300;
    private const           int       PanelHeight = 300;
    private static readonly Rectangle Dimensions  = new (GUI.GapSmall, 55, PanelWidth, PanelHeight);
    
    // State
    private Entity        entity;
    private List<InfoTab> tabs;
    private int           currentTabIndex = 0;
    
    // TODO: add 20 to the position for every subsequent dialog
    public Dialog_Info(Entity entity) : base(Dimensions) {
        ShowCloseX          = true;
        DismissOnRightClick = true;
        Draggable           = true;
        HeaderHeight        = 22;
        
        this.entity = entity;
        this.tabs   = entity.Components.Select(comp => comp.GetInfoTab()).Where(tab => tab != null).ToList();
    }

    public override void DoWindowContents() {
        base.DoWindowContents();

        var indent = GUI.GapSmall;
        var curY = 0;

        if (tabs.NullOrEmpty()) return;
        
        tabs[currentTabIndex].drawTabContents(GetRect().ContractedBy(GUI.GapTiny));
        // if (tabs.Count == 1) {
        // }
        
        // TODO: Tab rendering
    }
}