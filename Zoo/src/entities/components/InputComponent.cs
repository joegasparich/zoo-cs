using System.Numerics;

namespace Zoo.entities; 

public class InputComponent : Component {
    // State
    public Vector2 InputVector;
    
    public InputComponent(Entity entity, ComponentData? data) : base(entity, data) {}

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("inputVector", ref InputVector);
    }
}