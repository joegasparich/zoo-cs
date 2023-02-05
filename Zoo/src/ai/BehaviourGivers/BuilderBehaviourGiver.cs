using Zoo.entities;

namespace Zoo.ai;

public class BuilderBehaviourGiver {
    public static Behaviour Get(Staff staff) {
        if (Find.World.Blueprints.Blueprints.Values.Where(bp => !bp.Reserved).Any())
            return new BuildBlueprintBehaviour(staff);

        return new IdleBehaviour(staff);
    }
}