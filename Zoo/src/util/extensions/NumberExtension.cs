namespace Zoo.util; 

public static class NumberExtension {
    public static bool NearlyEquals(this float a, float b) {
        return Math.Abs(a - b) < 0.0001f;
    }
    public static int RoundToInt(this float f) => (int)MathF.Round(f);
    public static int FloorToInt(this float f) => (int)MathF.Floor(f);
    public static int CeilToInt(this  float f) => (int)MathF.Ceiling(f);
    public static string ToStringPercent(this float number, int decimalPlaces) {
        return (number * 100).ToString($"F{decimalPlaces}") + "%";
    }
}