using Zoo.entities;

namespace Zoo.ai; 

public class GuestBehaviourGiver {
    public static Behaviour Get(Guest guest) {
        return new IdleBehaviour(guest);
    }
}