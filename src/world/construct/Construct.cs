using Godot;
using System.Threading.Tasks;

[GlobalClass]
public partial class Construct : Node3D, IHaveBounds
{
	[Export] public bool IsGlobal { get; private set; }
	[Export] private ConstructGeneratorSettings constructGeneratorSettings;
	[Export] private SecondOrderDynamicsSettings rotSodSettings;
	[Export] private SecondOrderDynamicsSettings moveSodSettings;

	public ConstructData Data { get; private set; }
	public ConstructBlockService Blocks { get; private set; }

	private ConstructLoadingService loading;
	private ConstructPhysicsController physics;
	private ConstructVisualMotionController visualMotion;
	private ConstructMotionController motionController;
	private bool runPhysicsProcess = false;

	public void Initialize(int moduleSize, int seed, BlockStore blockStore, Material moduleMaterial, IWorldCollisionQuery collisionQuery)
	{
		var transform = new ConstructTransform((Vector3I)Position);
		var modules = new ConstructModuleController(moduleSize);
		var bounds = new ConstructBoundsController();

		Data = new ConstructData(transform, modules, bounds, blockStore, moduleMaterial);

		motionController = new ConstructMotionController(Data, collisionQuery);
		physics = new ConstructPhysicsController(Data, motionController, Position, IsGlobal);

		SecondOrderDynamics<float> rotSod = rotSodSettings.GetInstance(0);
		SecondOrderDynamics<Vector3> moveSod = moveSodSettings.GetInstance(Position);
		visualMotion = new ConstructVisualMotionController(Data, moveSod, rotSod);

		var visuals = new ConstructVisualsController(moduleSize, this);
		var moduleBuilder = new ConstructModuleBuilder();
		var generator = constructGeneratorSettings.CreateConstructGenerator(moduleSize, seed);

		Blocks = new ConstructBlockService(Data, moduleBuilder, visuals);
		loading = new ConstructLoadingService(Data, moduleBuilder, visuals, generator);

		Position = transform.WorldPos.Value;
		Rotation = visualMotion.Rotation;

		SetPhysicsProcess(true);
	}

	public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (visualMotion != null)
		{
			visualMotion.Update(delta);
			Position = visualMotion.Position;
			Rotation = visualMotion.Rotation;
		}

		physics.Update(delta);
	}

	// Block operations - thin delegation
	public void SetBlock(WorldGridPos worldPos, int blockId) => Blocks.SetBlock(worldPos, blockId);
	public void SetBlocks(WorldGridPos[] worldPositions, int[] blockIds) => Blocks.SetBlocks(worldPositions, blockIds);
	public bool TryGetBlock(WorldGridPos worldPos, out int blockId) => Blocks.TryGetBlock(worldPos, out blockId);

	// Loading operations - thin delegation
	public Task UpdateLoading(WorldGridPos worldPos, int renderDistance, int simulationDistance)
		=> loading.UpdateLoading(worldPos, renderDistance, simulationDistance);

	// IHaveBounds implementation
	public Vector3I GetRootPos() => Data.Transform.WorldPos;
	public Vector3I GetMin() => Data.Bounds.MinPos.ToWorld(Data.Transform);
	public Vector3I GetMax() => Data.Bounds.MaxPos.ToWorld(Data.Transform);
}
