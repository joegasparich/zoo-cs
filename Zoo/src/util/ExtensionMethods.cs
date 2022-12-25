namespace Zoo.util; 

public static class ExtensionMethods {
    public static int ToInt(this Enum e) {
        return Convert.ToInt32(e);
    }
}