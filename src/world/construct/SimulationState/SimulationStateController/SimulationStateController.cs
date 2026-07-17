using System;
using System.Collections.Generic;
using Godot;

public class SimulationStateController : ConstructController
{
    private SimulationState currentState;
    private SimulationMode currentMode;

    private Node3D parent;
    private List<Tuple<SimulationMode, float>> simulationModeDistances;
    private int moduleSize;

    private Dictionary<SimulationMode, SimulationState> states;

    public SimulationStateController(
        ConstructCore core,
        ConstructGenerator generator,
        ConstructModuleBuilder moduleBuilder,
        ConstructVoxelBlockVisualsController voxelVisuals,
        ConstructModelBlockVisualsController modelVisuals,

        IWorldQuery collisionQuery,
        SecondOrderDynamicsSettings rotSodSettings,
        SecondOrderDynamicsSettings moveSodSettings
        ) : base(core, generator, moduleBuilder, voxelVisuals, modelVisuals)
    {
        this.core = core;
        this.generator = generator;

        simulationModeDistances = GameSettings.Instance.SimulationModeDistances;
        moduleSize = GameSettings.Instance.ModuleSize;

        states[SimulationMode.ACTIVE] = new ActiveState(core, collisionQuery,
                rotSodSettings, moveSodSettings, generator, parent, UpdateLoadingInternal,
                voxelVisuals, modelVisuals);
        states[SimulationMode.APPROXIMATED] = new ApproximatedState(core, voxelVisuals, modelVisuals);
        states[SimulationMode.FROZEN] = new FrozenState(core, voxelVisuals, modelVisuals);
    }

    protected override void UpdateLoadingInternal(WorldGridPos loadPos)
    {
        WorldGridPos constPos = core.Data.GridTransform.WorldPos;
        float dist = (loadPos - (Vector3I)constPos).Length() / moduleSize;
        SimulationMode newMode = GetSimulationMode(dist);
        SetMode(newMode);
    }

    private void SetMode(SimulationMode newMode)
    {
        if (currentMode == newMode && currentState != null)
            return;

        currentState?.Exit();

        currentMode = newMode;
        currentState = states[newMode];

        currentState.Enter();
    }

    protected override void UpdateInternal(double delta) => currentState.Update(delta);

    private SimulationMode GetSimulationMode(float dist)
    {
        SimulationMode resultMode = simulationModeDistances[0].Item1;
        foreach ((var mode, var maxDist) in simulationModeDistances)
        {
            if (dist < maxDist)
                return mode;
            else
                resultMode = mode;
        }
        return resultMode;
    }
}
