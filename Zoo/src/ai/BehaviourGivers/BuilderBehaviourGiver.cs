using Zoo.entities;

namespace Zoo.ai;

public class BuilderBehaviourGiver {
    public static Behaviour Get(Staff staff) {
        if (Find.Zoo.ObjBlueprints.Any())
            return new BuildBlueprintBehaviour(staff);

        return new IdleBehaviour(staff);
    }
}