using Zoo.entities;

namespace Zoo.ai;

public class KeeperBehaviourGiver {
    public static Behaviour Get(Staff staff) {
        foreach (var exhibit in staff.AccesibleExhibits.Where(exhibit => !exhibit.Reserved)) {
            if (exhibit.NeedsMaintenance) return new MaintainExhibitBehaviour(staff, exhibit);
        }

        return new IdleBehaviour(staff);
    }
}