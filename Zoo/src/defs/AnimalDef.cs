using System.Text.Json.Serialization;

namespace Zoo.defs; 

public class AnimalDef : Def {
    public GraphicData       GraphicData;
    public bool              CanSwim;
    public DefRef<NeedDef>[] Needs;
}