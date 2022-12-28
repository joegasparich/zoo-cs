namespace Zoo.tools; 

public class NoTool : Tool {
    public override string   Name { get; } = "No Tool";
    public override ToolType Type { get; } = ToolType.None;

    public NoTool(ToolManager toolManager) : base(toolManager) {}
}