using Raylib_cs;

namespace Zoo.ui; 

public class Listing {
    // State
    private Rectangle rect;
    private float     listingY;
    private float     elementHeight;
    private float     gap;

    public Listing(Rectangle rect, float elementHeight = GUI.DefaultFontSize, float gap = GUI.GapTiny) {
        this.rect          = rect;
        listingY           = rect.y;
        this.elementHeight = elementHeight;
        this.gap           = gap;
    }

    public void Header(string text) {
        GUI.FontSize = GUI.HeaderFontSize;
        GUI.Label(new Rectangle(rect.x, listingY, rect.width, GUI.HeaderFontSize), text);
        listingY += GUI.HeaderFontSize + gap;
        GUI.Reset();
    }

    public void Label(string text, float? height = null) {
        GUI.Label(new Rectangle(rect.x, listingY, rect.width, height ?? elementHeight), text);
        listingY += height ?? elementHeight + gap;
    }
}