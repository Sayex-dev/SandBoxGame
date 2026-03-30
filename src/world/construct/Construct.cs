using Godot;
using System;

public partial class Construct : Node3D, IOctTreeObject
{
	public event Action<IOctTreeObject> BoundsChanged;

	public ConstructCore Core { get; private set; }
	public ConstructBlockService Blocks { get; private set; }
	public ConstructVisualsController Visuals { get; private set; }
	public ConstructModuleBuilder ModuleBuilder { get; private set; }

	private ConstructPhysicsController physics;
	private ConstructVisualMotionController visualMotion;
	private ConstructMotionController motionController;
	private SimulationStateController sim;

	public Construct(
		ConstructCreationSettings settings,
		int moduleSize,
		Material moduleMaterial,
		IWorldQuery collisionQuery,
		Node parent,
		Vector3I initialPosition = default,
		SimulationMode initialSimulationMode = SimulationMode.FROZEN
	)
	{
		var transform = new ConstructGridTransform(initialPosition);
		var modules = new ConstructModules(moduleSize);
		var bounds = new ConstructBounds();

		var physicsData = new ConstructPhysicsData()
		{
			PhysicsPosition = initialPosition,
			IsStatic = settings.IsGlobal
		};
		var data = new ConstructData(physicsData, transform, modules, bounds, moduleMaterial);

		data.GridTransform.Changed += OnSpatialChanged;
		data.Bounds.Changed += OnSpatialChanged;

		motionController = new ConstructMotionController(data, collisionQuery);
		physics = new ConstructPhysicsController(data, motionController);

		SecondOrderDynamics<float> rotSod = settings.RotSodSettings.GetInstance(0);
		SecondOrderDynamics<Vector3> moveSod = settings.MoveSodSettings.GetInstance(initialPosition);
		visualMotion = new ConstructVisualMotionController(data, moveSod, rotSod);

		Visuals = new ConstructVisualsController(moduleSize);
		ModuleBuilder = new ConstructModuleBuilder();

		Blocks = new ConstructBlockService(data);

		Core = new ConstructCore(data, Blocks, this);
		sim = new SimulationStateController(Core, collisionQuery, settings.RotSodSettings, settings.MoveSodSettings, moduleSize, initialSimulationMode);
	}

	public override void _PhysicsProcess(double delta)
	{
		sim.Update(delta);
	}

	public void ChangeSimulationState(SimulationMode mode) => sim.ChangeMode(mode);

	public void SetBlock(WorldGridPos worldPos, Block block) => Blocks.SetBlock(worldPos, block);
	public void SetBlocks(WorldGridPos[] worldPositions, Block[] blocks) => Blocks.SetBlocks(worldPositions, blocks);
	public bool TryGetBlock(WorldGridPos worldPos, out Block block) => Blocks.TryGetBlock(worldPos, out block);

	public Vector3I GetRootPos() => Core.Data.GridTransform.WorldPos;
	public Vector3I GetMin() => Core.Data.Bounds.MinPos.ToWorld(Core.Data.GridTransform);
	public Vector3I GetMax() => Core.Data.Bounds.MaxPos.ToWorld(Core.Data.GridTransform);

	private void OnSpatialChanged()
	{
		BoundsChanged?.Invoke(this);
	}
}
