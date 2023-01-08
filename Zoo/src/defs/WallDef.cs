namespace Zoo.defs; 

public enum WallType {
    Fence
}

public class WallDef : Def {
    public WallType    Type;
    public GraphicData GraphicData;
    public bool        Solid = true;
}