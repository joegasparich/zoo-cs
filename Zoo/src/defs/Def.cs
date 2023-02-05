namespace Zoo.defs;

public class Def {
    // Config
    public string  Class;
    public bool    Abstract = false;
    public string? Inherits;
    
    public string       Id;
    public string?      Name;

    // Properties
    public Type DefType => Type.GetType("Zoo.defs." + Class);
}