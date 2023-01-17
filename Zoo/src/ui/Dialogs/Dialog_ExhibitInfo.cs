using Raylib_cs;
using Zoo.world;

namespace Zoo.ui; 

public class Dialog_ExhibitInfo : Dialog {
    // Constants
    private const           int       PanelWidth  = 300;
    private const           int       PanelHeight = 300;
    private static readonly Rectangle Dimensions  = new (GUI.GapSmall, 55, PanelWidth, PanelHeight);
    private const           int       TabWidth    = 50;
    private const           int       TabHeight   = 20;
    
    // State
    private static int openInfoDialogs = 0;
    
    private Exhibit exhibit;

    public Dialog_ExhibitInfo(Exhibit exhibit) : base(Dimensions) {
        ShowCloseX = true;
        DismissOnRightClick = true;
        Draggable           = true;
        HeaderHeight        = 22;
        
        this.exhibit = exhibit;
        
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

        var listing = new Listing(GetRect());
        listing.Header(exhibit.Name);
        listing.Label($"Animals: {exhibit.ContainedAnimals.Count}");
    }
}