namespace Zoo.tools; 

public class Tool_None : Tool {
    public override string   Name => "No Tool";
    public override ToolType Type => ToolType.None;

    public Tool_None(ToolManager tm) : base(tm) {}
}