using System;
using System.Collections.Generic;
using Godot;

public class SimulationStateController : IConstructController
{
    private ConstructCore core;
    private SimulationState currentState;
    private SimulationMode currentMode;

    private IWorldQuery collisionQuery;
    private SecondOrderDynamicsSettings rotSodSettings;
    private SecondOrderDynamicsSettings moveSodSettings;
    private ConstructGenerator generator;
    private Node3D parent;
    private List<Tuple<SimulationMode, float>> simulationModeDistances;
    private int moduleSize;

    // Controllers that survive state transitions
    private ConstructVisualsController visuals;
    private ConstructModelBlockController modelBlocks;
    private bool controllersInitialized;

    public SimulationStateController(
        ConstructCore core,
        IWorldQuery collisionQuery,
        SecondOrderDynamicsSettings rotSodSettings,
        SecondOrderDynamicsSettings moveSodSettings,
        ConstructGenerator generator,
        Node3D parent)
    {
        this.core = core;
        this.collisionQuery = collisionQuery;
        this.rotSodSettings = rotSodSettings;
        this.moveSodSettings = moveSodSettings;
        this.generator = generator;
        this.parent = parent;

        simulationModeDistances = GameSettings.Instance.SimulationModeDistances;
        moduleSize = GameSettings.Instance.ModuleSize;
    }

    public void UpdateLoading(WorldGridPos loadPos)
    {
        WorldGridPos constPos = core.Data.GridTransform.WorldPos;
        float dist = (loadPos - (Vector3I)constPos).Length() / moduleSize;
        SimulationMode newMode = GetSimulationMode(dist);

        if (currentMode == newMode && currentState != null)
            return;

        currentState?.Exit();

        // Initialize shared controllers on first ActiveState entry
        if (!controllersInitialized && newMode == SimulationMode.ACTIVE)
        {
            visuals = new ConstructVisualsController(core.Data.Modules);
            modelBlocks = new ConstructModelBlockController(parent, core.Data);
            parent.AddChild(visuals);
            controllersInitialized = true;
        }

        currentState = newMode switch
        {
            SimulationMode.ACTIVE => new ActiveState(core, collisionQuery,
                rotSodSettings, moveSodSettings, generator, parent, UpdateLoading,
                visuals, modelBlocks),
            SimulationMode.APPROXIMATED => new ApproximatedState(core, visuals, modelBlocks),
            SimulationMode.FROZEN => new FrozenState(core, visuals, modelBlocks),
            _ => throw new ArgumentException($"Unknown mode: {newMode}")
        };

        currentState.Enter();
        currentMode = newMode;
    }

    public void Update(double delta) => currentState.Update(delta);

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
