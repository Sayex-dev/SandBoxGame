using Godot;

public class FrozenState : SimulationState
{
    public FrozenState(ConstructCore core,
        ConstructVoxelBlockVisualsController visuals = null,
        ConstructModelBlockVisualsController modelBlocks = null)
        : base(core, visuals, modelBlocks) { }

    // No Update — visuals stay visible but completely frozen
}
