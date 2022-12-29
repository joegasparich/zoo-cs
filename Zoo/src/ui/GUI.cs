using System.Numerics;
using Raylib_cs;
using Zoo.util;

namespace Zoo.ui;

public enum AlignMode {
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public static class GUI {
    public const int GapSmall = 10;

    public static           Color     TextColour     = Color.BLACK;
    public static           AlignMode TextAlign      = AlignMode.TopLeft;
    public static           int       FontSize       = 10;
    private static readonly Color     HighlightColor = new Color(255, 255, 255, 150);
    public static readonly Color     UIButtonColour = new Color(230, 230, 230, 255);

    private static Rectangle MaintainAspectRatio(Rectangle rect, Texture2D texture, Rectangle? source = null) {
        source ??= new Rectangle(0, 0, 1, 1);
        
        var texWidth = texture.width * source.Value.width;
        var texHeight = texture.height * source.Value.height;

        if (texWidth < texHeight) {
            var width = rect.width * (texWidth / texHeight);
            return new Rectangle(rect.x + (rect.width - width) / 2, rect.y, width, rect.height);
        }
        if (texWidth > texHeight) {
            var height = rect.height * (texHeight / texWidth);
            return new Rectangle(rect.x, rect.y + (rect.height - height) / 2, rect.width, height);
        }

        return rect;
    }
    
    // Draw functions
    public static void DrawRect(Rectangle rect, Color col) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawRectangle(absRect.x.RoundToInt(), absRect.y.RoundToInt(), absRect.width.RoundToInt(), absRect.height.RoundToInt(), col);
    }
    
    public static void DrawBorder(Rectangle rect, int thickness, Color col) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawRectangleLinesEx(absRect, thickness, col);
    }

    public static void DrawTexture(Rectangle rect, Texture2D texture, Color? col = null) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawTexturePro(
            texture,
            new Rectangle(0, 0, texture.width, texture.height),
            MaintainAspectRatio(absRect, texture), 
            new Vector2(0, 0),
            0,
            col ?? Color.WHITE
        );
    }
    
    public static void DrawSubTexture(Rectangle rect, Texture2D texture, Rectangle source, Color? col = null) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawTexturePro(
            texture,
            new Rectangle(
                texture.width * source.x,
                texture.height * source.y,
                texture.width * source.width,
                texture.height * source.height
            ),
            MaintainAspectRatio(absRect, texture, source), 
            new Vector2(0, 0),
            0,
            col ?? Color.WHITE
        );
    }

    public static void DrawTextureNPatch(Rectangle rect, Texture2D texture, int cornerSize, Color? col = null) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) return;

        var nPatchInfo = new NPatchInfo {
            source = new Rectangle(0, 0, texture.width, texture.height),
            left = cornerSize,
            top = cornerSize,
            right = cornerSize,
            bottom = cornerSize,
            layout = NPatchLayout.NPATCH_NINE_PATCH
        };

        Raylib.DrawTextureNPatch(
            texture,
            nPatchInfo,
            Find.UI.GetAbsRect(rect),
            new Vector2(0, 0),
            0,
            col ?? Color.WHITE
        );
    }

    public static void DrawText(Rectangle rect, string text) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) return;

        var absRect   = Find.UI.GetAbsRect(rect);
        var textWidth = Raylib.MeasureText(text, FontSize);

        Vector2 drawPos = TextAlign switch {
            AlignMode.TopLeft => new Vector2(absRect.x, absRect.y),
            AlignMode.TopCenter => new Vector2(absRect.x + (absRect.width - textWidth) / 2, absRect.y),
            AlignMode.TopRight => new Vector2(absRect.x + (absRect.width - textWidth), absRect.y),
            AlignMode.MiddleLeft => new Vector2(absRect.x, absRect.y + (absRect.height - FontSize) / 2),
            AlignMode.MiddleCenter => new Vector2(absRect.x + (absRect.width - textWidth) / 2, absRect.y + (absRect.height - FontSize) / 2),
            AlignMode.MiddleRight => new Vector2(absRect.x + (absRect.width - textWidth), absRect.y + (absRect.height - FontSize) / 2),
            AlignMode.BottomLeft => new Vector2(absRect.x, absRect.y + (absRect.height - FontSize)),
            AlignMode.BottomCenter => new Vector2(absRect.x + (absRect.width - textWidth) / 2, absRect.y + (absRect.height - FontSize)),
            AlignMode.BottomRight => new Vector2(absRect.x + (absRect.width - textWidth), absRect.y + (absRect.height - FontSize))
        };
        
        Raylib.DrawText(text, drawPos.X.RoundToInt(), drawPos.Y.RoundToInt(), FontSize, TextColour);
    }
    
    // Input functions
    public static bool ClickableArea(Rectangle rect) {
        if (Find.UI.CurrentEvent != UIEvent.Input) return false;

        return Find.Input.GetCurrentEvent().mouseDown == MouseButton.MOUSE_BUTTON_LEFT && Find.UI.IsMouseOverRect(rect);
    }
    
    public static bool HoverableArea(Rectangle rect) {
        return Find.UI.IsMouseOverRect(rect);
    }
    
    // Widgets
    public static bool ButtonText(Rectangle rect, string text) {
        return ButtonText(rect, text, UIButtonColour);
    }

    public static bool ButtonText(Rectangle rect, string text, Color col) {
        DrawRect(rect, col);
        HighlightMouseover(rect);
        DrawText(rect, text);

        if (HoverableArea(rect)) {
            Find.UI.SetCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);
        }

        return ClickableArea(rect);
    }

    public static void HighlightMouseover(Rectangle rect) {
        if (HoverableArea(rect)) {
            DrawRect(rect, HighlightColor);
        }
    }
}