using Zoo.entities;

namespace Zoo.ai;

public abstract class Behaviour : ISerialisable {
    // References
    protected Actor actor;
    
    // State
    public bool completed;
    public int  expireTick = -1;
    
    // Properties
    public bool                Expired => expireTick > 0 && Game.Ticks > expireTick;
    public PathFollowComponent Pather  => actor.GetComponent<PathFollowComponent>();

    public Behaviour() {}
    public Behaviour(Actor actor) {
        this.actor = actor;
    }

    public virtual void Start()      { }
    public virtual void OnComplete() {
        Pather.ResetPath();
    }
    public virtual void OnExpire() {
        Pather.ResetPath();
    }
    public virtual void Update()     { }
    public virtual void Serialise() {
        Find.SaveManager.ArchiveValue("actor", () => actor.Id, id => actor = Game.GetEntityById(id) as Actor);
        Find.SaveManager.ArchiveValue("completed",  ref completed);
        Find.SaveManager.ArchiveValue("expireTick", ref expireTick);
    }
}