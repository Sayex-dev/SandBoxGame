using Godot;

public class FrozenState : SimulationState
{
    public FrozenState(ConstructCore core,
        ConstructVisualsController visuals = null,
        ConstructModelBlockController modelBlocks = null)
        : base(core, visuals, modelBlocks) { }

    // No Update — visuals stay visible but completely frozen
}
