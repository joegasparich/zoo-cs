namespace Zoo.util; 

public struct IntVec2 : IEquatable<IntVec2> {
    public int X;
    public int Y;
    
    public static IntVec2 Zero => new IntVec2(0, 0);
    public static IntVec2 One => new IntVec2(1, 1);
    
    public IntVec2() {
        X = 0;
        Y = 0;
    }
    public IntVec2(int n) {
        X = n;
        Y = n;
    }
    public IntVec2(int x, int y) {
        X = x;
        Y = y;
    }
    
    public static IntVec2 operator +(IntVec2 a, IntVec2 b) {
        return new IntVec2(a.X + b.X, a.Y + b.Y);
    }
    public static IntVec2 operator -(IntVec2 a, IntVec2 b) {
        return new IntVec2(a.X - b.X, a.Y - b.Y);
    }
    public static IntVec2 operator *(IntVec2 a, IntVec2 b) {
        return new IntVec2(a.X * b.X, a.Y * b.Y);
    }
    public static IntVec2 operator *(IntVec2 a, int b) {
        return new IntVec2(a.X * b, a.Y * b);
    }
    public static IntVec2 operator /(IntVec2 a, int b) {
        return new IntVec2(a.X / b, a.Y / b);
    }
    public static bool operator ==(IntVec2 a, IntVec2 b) {
        return a.Equals(b);
    }
    public static bool operator !=(IntVec2 a, IntVec2 b) {
        return !a.Equals(b);
    }

    public bool Equals(IntVec2 other) {
        return X == other.X && Y == other.Y;
    }
}