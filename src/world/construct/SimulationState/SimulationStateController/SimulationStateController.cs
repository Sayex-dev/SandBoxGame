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

        currentState = newMode switch
        {
            SimulationMode.ACTIVE => new ActiveState(core, collisionQuery,
                rotSodSettings, moveSodSettings, generator, parent, UpdateLoading),
            SimulationMode.APPROXIMATED => new ApproximatedState(core),
            SimulationMode.FROZEN => new FrozenState(core),
            _ => throw new ArgumentException($"Unknown mode: {newMode}")
        };

        currentState.Enter();
        currentMode = newMode;
    }

    public void Update(double delta) => currentState.Update(delta);
    public void SetBlock(Block block, ConstructGridPos pos) => currentState.SetBlock(block, pos);
    public void RemoveBlock(ConstructGridPos pos) => currentState.RemoveBlock(pos);
    public bool TryGetBlock(ConstructGridPos pos, out Block block) => currentState.TryGetBlock(pos, out block);


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