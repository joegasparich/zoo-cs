namespace Zoo.defs;

public class Def {
    // Config
    public string  Class; // TODO: Can prob get rid of Class and Inherits (and Abstract?)
    public bool    Abstract = false;
    public string? Inherits;
    
    public string  Id;
    public string? Name;
}