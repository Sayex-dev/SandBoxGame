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
    public virtual void SetBlock(Block block, ConstructGridPos pos) => core.Blocks.SetBlock(pos, block);
    public virtual void RemoveBlock(ConstructGridPos pos) => core.Blocks.SetBlock(pos, new Block());
    public virtual bool TryGetBlock(ConstructGridPos pos, out Block block)
    {
        return core.Blocks.TryGetBlock(pos, out block);
    }

    public abstract Vector3 GetPosition();
    public abstract Vector3 GetRotation();
}