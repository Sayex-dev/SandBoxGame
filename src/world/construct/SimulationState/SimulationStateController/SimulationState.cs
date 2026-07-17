using Godot;

public abstract class SimulationState
{
    protected ConstructCore core;
    protected ConstructVoxelBlockVisualsController visuals;
    protected ConstructModelBlockVisualsController modelBlocks;

    public SimulationState(ConstructCore core,
        ConstructVoxelBlockVisualsController visuals = null,
        ConstructModelBlockVisualsController modelBlocks = null)
    {
        this.core = core;
        this.visuals = visuals;
        this.modelBlocks = modelBlocks;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(double delta) { }
}
