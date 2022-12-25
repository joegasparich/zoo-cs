namespace Zoo; 

public abstract class Scene {
    public string Name { get; set; }

    protected Scene(string name) {
        Name = name;
    }

    public virtual void Start()                 {}
    public virtual void PreUpdate()             {}
    public virtual void Update()                {}
    public virtual void PostUpdate()            {}
    public virtual void Render()                {}
    public virtual void RenderLate()            {}
    public virtual void OnGUI()                 {}
    public virtual void OnInput(InputEvent evt) {}
    public virtual void Stop()                  {}
}