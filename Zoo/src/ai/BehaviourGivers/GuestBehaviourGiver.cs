using Zoo.entities;
using Zoo.util;

namespace Zoo.ai; 

public class GuestBehaviourGiver {
    public static Behaviour Get(Guest guest) {
        if (guest.ExhibitsViewed.Count == Find.World.Exhibits.Exhibits.Count()
            && Rand.Chance(0.1f)) {
            return new LeaveBehaviour(guest);
        }
        
        if (Rand.Chance(0.2f)) {
            // TODO: Change this to only look for nearby exhibits
            var randomExhibit = Find.World.Exhibits.Exhibits.RandomElement();
            
            if (randomExhibit != null && !guest.ExhibitsViewed.Contains(randomExhibit)) {
                return new ViewExhibitBehaviour(guest, randomExhibit);
            }
        }
        
        return new IdleBehaviour(guest);
    }
}