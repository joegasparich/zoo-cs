namespace Zoo.util; 

public class Rand {
    public static int randInt(int min, int max) {
        Random rand = new Random();
        int randomNum = rand.Next((max - min) + 1) + min;
        return randomNum;
    }
    
    public static byte randByte() {
        Random rand = new Random();
        byte randomNum = (byte)rand.Next(0, 255);
        return randomNum;
    }
    
    public static float randFloat() {
        Random rand = new Random();
        float randomNum = (float)rand.NextDouble();
        return randomNum;
    }
}