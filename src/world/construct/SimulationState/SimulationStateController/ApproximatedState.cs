using Godot;

public class ApproximatedState : SimulationState
{
    public ApproximatedState(ConstructCore core,
        ConstructVisualsController visuals = null,
        ConstructModelBlockController modelBlocks = null)
        : base(core, visuals, modelBlocks) { }

    // No Update — visuals stay visible but no physics/motion simulation
}
