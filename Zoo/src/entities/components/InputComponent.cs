using System.Numerics;

namespace Zoo.entities; 

public class InputComponent : Component {
    public Vector2 InputVector;
    
    public InputComponent(Entity entity) : base(entity) {}

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.SerialiseValue("inputVector", ref InputVector);
    }
}