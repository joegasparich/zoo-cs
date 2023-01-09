using System.Text.Json.Serialization;

namespace Zoo.defs; 

public class AnimalDef : EntityDef {
    public GraphicData       GraphicData;
    public bool              CanSwim;
    public DefRef<NeedDef>[] Needs;
}