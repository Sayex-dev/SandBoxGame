using Godot;

public abstract class SimulationState
{
    protected ConstructCore core;

    public SimulationState(ConstructCore core)
    {
        this.core = core;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(double delta) { }
    public virtual void AddBlock(Block block, ConstructGridPos pos)
    {
        core.Blocks.SetBlock(pos, block);
    }

    public abstract Vector3 GetPosition();
    public abstract Vector3 GetRotation();
}