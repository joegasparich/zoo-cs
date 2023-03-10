using Zoo.defs;

namespace Zoo.entities;

public class ConsumableComponentData : ComponentData {
    public override Type CompClass => typeof(ConsumableComponent);
    
    public DefRef<NeedDef> Need;
    public float           MaxQuantity;
}

public class ConsumableComponent : Component {
    public static Type DataType => typeof(ConsumableComponentData);

    // State
    private float quantity;
    
    // Properties
    public ConsumableComponentData Data => (ConsumableComponentData)data;

    public ConsumableComponent(Entity entity, ComponentData? data = null) : base(entity, data) {
        quantity = Data.MaxQuantity;
    }

    public float Consume(float requestedAmount) {
        var consumed = MathF.Min(requestedAmount, quantity);
        
        quantity -= consumed;
        if (quantity <= 0) {
            entity.Destroy();
        }

        return consumed;
    }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveValue("quantity", ref quantity);
    }
}