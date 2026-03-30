using Godot;
using System;

public partial class Construct : IOctTreeObject
{
	public event Action<IOctTreeObject> BoundsChanged;

	public ConstructData Data { get; private set; }
	public ConstructBlockService Blocks { get; private set; }
	public ConstructVisualsController Visuals { get; private set; }
	public ConstructModuleBuilder ModuleBuilder { get; private set; }

	private ConstructPhysicsController physics;
	private ConstructVisualMotionController visualMotion;
	private ConstructMotionController motionController;

	public void Initialize(
		CosntructCreationSettings settings,
		int moduleSize, 
		Material moduleMaterial, 
		IWorldQuery collisionQuery, 
		Vector3I initialPosition = default,
		SimulationMode initialSimulationMode = SimulationMode.FROZEN
	)
	{
		var transform = new ConstructTransform(initialPosition);
		var modules = new ConstructModules(moduleSize);
		var bounds = new ConstructBounds();

		var physicsData = new ConstructPhysicsData();
		Data = new ConstructData(physicsData, transform, modules, bounds, moduleMaterial);

		Data.Transform.Changed += OnSpatialChanged;
		Data.Bounds.Changed += OnSpatialChanged;

		motionController = new ConstructMotionController(Data, collisionQuery);
		physics = new ConstructPhysicsController(Data, motionController);

		SecondOrderDynamics<float> rotSod = settings..GetInstance(0);
		SecondOrderDynamics<Vector3> moveSod = moveSodSettings.GetInstance(Position);
		visualMotion = new ConstructVisualMotionController(Data, moveSod, rotSod);

		Visuals = new ConstructVisualsController(moduleSize);
		ModuleBuilder = new ConstructModuleBuilder();

		Blocks = new ConstructBlockService(Data);

		Position = transform.WorldPos.Value;
		Rotation = visualMotion.Rotation;

		simulationMode = initialSimulationMode;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position = visualMotion.Position;
		Rotation = new Vector3(0, Data.Transform.YRotation, 0);
	}

	public void SetBlock(WorldGridPos worldPos, Block block) => Blocks.SetBlock(worldPos, block);
	public void SetBlocks(WorldGridPos[] worldPositions, Block[] blocks) => Blocks.SetBlocks(worldPositions, blocks);
	public bool TryGetBlock(WorldGridPos worldPos, out Block block) => Blocks.TryGetBlock(worldPos, out block);

	public Vector3I GetRootPos() => Data.Transform.WorldPos;
	public Vector3I GetMin() => Data.Bounds.MinPos.ToWorld(Data.Transform);
	public Vector3I GetMax() => Data.Bounds.MaxPos.ToWorld(Data.Transform);

	private void OnSpatialChanged()
	{
		BoundsChanged?.Invoke(this);
	}
}
