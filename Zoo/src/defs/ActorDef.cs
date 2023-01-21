using Zoo.world;

namespace Zoo.defs; 

public class ActorDef : EntityDef {
    public AccessibilityType Accessibility = AccessibilityType.NoWaterIgnorePaths;
    
    public bool CanSwim => Accessibility == AccessibilityType.NoSolid || Accessibility == AccessibilityType.NoSolidIgnorePaths;
}