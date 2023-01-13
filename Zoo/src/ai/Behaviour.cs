namespace Zoo.ai; 

public abstract class Behaviour : ISerialisable {
    public virtual void Start()     { }
    public virtual void End()       { }
    public virtual void Update()    { }
    public virtual void Serialise() { }
}