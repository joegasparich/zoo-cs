using Raylib_cs;
using Zoo.util;

namespace Zoo.ui;

public class Widget_ZooInfo : Window {
    // Constants
    private const           int           Width                  = 200;
    private const           int           Height                 = 24;
    private static readonly Rectangle     Dimensions             = new (Game.ScreenWidth - Width, Game.ScreenHeight - Height, Width, Height);
    private static readonly CachedTexture Pause = new("assets/textures/ui/pause.png");
    private static readonly CachedTexture Play = new("assets/textures/ui/play.png");
    private static readonly CachedTexture Fast = new("assets/textures/ui/fast.png");
    private static readonly CachedTexture Faster = new("assets/textures/ui/faster.png");

    public Widget_ZooInfo() : base(Dimensions) {}

    public override void DoWindowContents() {
        base.DoWindowContents();

        GUI.DrawRect(GetRect(), Color.WHITE);

        using (new TextBlock(AlignMode.MiddleCenter)) {
            var curX = 0;
            GUI.Label(new Rectangle(curX, 0, 50, Height), $"${Find.Zoo.Cash}");
            curX += 50 + GUI.GapSmall;

            if (GUI.ButtonIcon(new Rectangle(curX, 4, 16, 16), Pause.Texture))
                Game.TogglePause();
            curX += 16 + GUI.GapTiny;

            if (GUI.ButtonIcon(new Rectangle(curX, 4, 16, 16), Play.Texture))
                Game.SetTickRate(Game.TickRate.Normal);
            curX += 16 + GUI.GapTiny;

            if (GUI.ButtonIcon(new Rectangle(curX, 4, 16, 16), Fast.Texture))
                Game.SetTickRate(Game.TickRate.Fast);
            curX += 16 + GUI.GapTiny;

            if (GUI.ButtonIcon(new Rectangle(curX, 4, 16, 16), Faster.Texture))
                Game.SetTickRate(Game.TickRate.Faster);
        }
    }
}