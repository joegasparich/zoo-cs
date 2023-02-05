using Zoo.entities;

namespace Zoo.ai;

public class BuilderBehaviourGiver {
    public static Behaviour Get(Staff staff) {
        return new IdleBehaviour(staff);
    }
}