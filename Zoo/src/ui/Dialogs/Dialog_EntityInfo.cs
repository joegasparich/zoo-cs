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

public class Dialog_EntityInfo : Dialog {
    // Constants
    private const           int       PanelWidth  = 300;
    private const           int       PanelHeight = 300;
    private static readonly Rectangle Dimensions  = new (GUI.GapSmall, 55, PanelWidth, PanelHeight);
    private const           int       TabWidth    = 50;
    private const           int       TabHeight   = 20;
    
    // State
    private static int openInfoDialogs = 0;
    
    private Entity        entity;
    private List<InfoTab> tabs;
    private int           currentTabIndex = 0;
    
    public Dialog_EntityInfo(Entity entity) : base(Dimensions) {
        ShowCloseX          = true;
        DismissOnRightClick = true;
        Draggable           = true;
        HeaderHeight        = 22;
        
        this.entity = entity;
        this.tabs   = entity.GetInfoTabs();

        AbsRect = new Rectangle(
            AbsRect.x + openInfoDialogs * 20,
            AbsRect.y + openInfoDialogs * 20,
            AbsRect.width,
            AbsRect.height
        );

        openInfoDialogs++;
    }

    public override void OnClose() {
        base.OnClose();

        openInfoDialogs--;
    }

    public override void DoWindowContents() {
        base.DoWindowContents();

        if (tabs.NullOrEmpty()) return;
        
        if (tabs.Count == 1) {
            tabs[0].drawTabContents(GetRect().ContractedBy(GUI.GapTiny));
        } else {
            using (new TextBlock(AlignMode.MiddleCenter)) {
                for (var i = 0; i < tabs.Count; i++) {
                    var tab = tabs[i];
                    if (GUI.ButtonText(new Rectangle(i * TabWidth, 0, TabWidth, TabHeight), tab.tabName, selected: currentTabIndex == i))
                        currentTabIndex = i;
                }
            }
            tabs[currentTabIndex].drawTabContents(new Rectangle(0, TabHeight, GetWidth(), GetHeight() - TabHeight).ContractedBy(GUI.GapTiny));
        }
    }
}