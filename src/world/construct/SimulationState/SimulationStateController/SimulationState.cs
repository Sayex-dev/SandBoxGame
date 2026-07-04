using Godot;

public abstract class SimulationState
{
    protected ConstructCore core;
    protected ConstructVisualsController visuals;
    protected ConstructModelBlockController modelBlocks;

    public SimulationState(ConstructCore core,
        ConstructVisualsController visuals = null,
        ConstructModelBlockController modelBlocks = null)
    {
        this.core = core;
        this.visuals = visuals;
        this.modelBlocks = modelBlocks;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(double delta) { }
}
