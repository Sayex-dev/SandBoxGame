using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public class SimulationStateController
{
    private ConstructCore core;
    private SimulationState currentState;
    private SimulationMode currentMode;

    private IWorldQuery collisionQuery;
    private SecondOrderDynamicsSettings rotSodSettings;
    private SecondOrderDynamicsSettings moveSodSettings;
    private int moduleSize;

    public SimulationStateController(
        ConstructCore core,
        IWorldQuery collisionQuery,
        SecondOrderDynamicsSettings rotSodSettings,
        SecondOrderDynamicsSettings moveSodSettings,
        int moduleSize,
        SimulationMode initialMode)
    {
        this.core = core;
        this.collisionQuery = collisionQuery;
        this.rotSodSettings = rotSodSettings;
        this.moveSodSettings = moveSodSettings;
        this.moduleSize = moduleSize;

        ChangeMode(initialMode);
    }

    public void ChangeMode(SimulationMode newMode)
    {
        if (currentMode == newMode && currentState != null)
            return;

        currentState?.Exit();

        // Create appropriate state
        currentState = newMode switch
        {
            SimulationMode.ACTIVE => new ActiveState(core, collisionQuery,
                rotSodSettings, moveSodSettings, moduleSize),
            SimulationMode.DORMANT => new DormantState(core),
            SimulationMode.FROZEN => new FrozenState(core),
            _ => throw new ArgumentException($"Unknown mode: {newMode}")
        };

        currentState.Enter();
        currentMode = newMode;
    }

    public void Update(double delta) => currentState.Update(delta);
    public Vector3 GetPosition() => currentState.GetPosition();
    public Vector3 GetRotation() => currentState.GetRotation();
    public void AddBlock(Block block, ConstructGridPos pos) => currentState.AddBlock(block, pos);
}