using System.Numerics;

namespace Zoo.entities; 

public class InputComponent : Component {
    public Vector2 InputVector { get; protected set; }
    
    public InputComponent(Entity entity) : base(entity) {}
}