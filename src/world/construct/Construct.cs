using Godot;
using System;

public partial class Construct : Node3D, IOctTreeObject
{
	public event Action<IOctTreeObject> BoundsChanged;

	public ConstructCore Core { get; private set; }
	public ConstructBlockService Blocks { get; private set; }

	private IConstructController sim;

	public static Construct GetInitializedConstruct(
		ConstructCreationSettings settings,
		IWorldQuery collisionQuery,
		Vector3I initialPosition = default
	)
	{
		Construct construct = new Construct();

		int seed = GameSettings.Instance.Seed;
		var transform = new ConstructGridTransform(initialPosition);
		var modules = new ConstructModules();
		var bounds = new ConstructBounds();

		var physicsData = new ConstructPhysicsData()
		{
			PhysicsPosition = initialPosition,
			IsStatic = settings.IsGlobal
		};
		var data = new ConstructData(physicsData, transform, modules, bounds);

		data.GridTransform.Changed += construct.OnSpatialChanged;
		data.Bounds.Changed += construct.OnSpatialChanged;

		ConstructMotionController motionController = new ConstructMotionController(data, collisionQuery);
		var physics = new ConstructPhysicsController(data, motionController);

		var rotSod = settings.RotSodSettings.GetInstance(0);
		var moveSod = settings.MoveSodSettings.GetInstance(initialPosition);
		var visualMotion = new ConstructVisualMotionController(data, moveSod, rotSod);

		construct.Blocks = new ConstructBlockService(data);
		construct.Core = new ConstructCore(data, construct.Blocks, construct);
		ConstructGenerator generator = settings.ConstructGeneratorSettings.CreateConstructGenerator(seed);

		if (settings.IsGlobal)
		{
			construct.sim = new GlobalConstructController(construct.Core, generator, construct);
		}
		else
		{
			construct.sim = new SimulationStateController(construct.Core, collisionQuery, settings.RotSodSettings, settings.MoveSodSettings, generator, construct);
		}

		return construct;
	}

	public override void _PhysicsProcess(double delta)
	{
		sim.Update(delta);
	}

	public void UpdateLoading(WorldGridPos loadPos) => sim.UpdateLoading(loadPos);

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
