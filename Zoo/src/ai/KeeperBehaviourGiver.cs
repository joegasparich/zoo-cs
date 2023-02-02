using Zoo.entities;

namespace Zoo.ai; 

public class KeeperBehaviourGiver {
    public static Behaviour Get(Actor actor) {
        return new IdleBehaviour(actor);
    }
}