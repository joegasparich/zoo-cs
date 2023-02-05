using Zoo.entities;

namespace Zoo.ai;

public class BuilderBehaviourGiver {
    public static Behaviour Get(Staff staff) {
        if (Find.World.Blueprints.Blueprints.Values.Where(bp => !bp.Reserved).Any())
            return new BuildBlueprintBehaviour(staff);

        // Return to zoo area if got stuck
        if (!staff.Area.IsZooArea)
            return new ReturnToZooAreaBehaviour(staff);

        return new IdleBehaviour(staff);
    }
}