using Godot;
using System;
using System.Linq;

public partial class Construct : Node3D, IOctTreeObject
{
	public event Action<IOctTreeObject> BoundsChanged;

	private IConstructController sim;


	public static Construct GetInitializedConstruct(
		ConstructCreationSettings settings,
		IWorldQuery collisionQuery,
		Vector3I initialPosition = default
	)
	{
		// Todo: Dependency Injection.
		Construct construct = new Construct();

		int seed = GameSettings.Instance.Seed;
		var transform = new ConstructGridTransformController((WorldGridPos)initialPosition);
		var modules = new ConstructBlockController();
		var bounds = new ConstructBoundsController(modules);

		var physicsData = new ConstructPhysicsData()
		{
			PhysicsPosition = initialPosition,
			IsStatic = settings.IsGlobal
		};
		var data = new ConstructData(physicsData, transform, modules, bounds);

		data.GridTransform.Changed += construct.OnSpatialChanged;
		data.Bounds.Changed += construct.OnSpatialChanged;

		construct.Core = new ConstructCore(data);
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

	public override void _ExitTree()
	{
		Core.Data.GridTransform.Changed += OnSpatialChanged;
		Core.Data.Bounds.Changed += OnSpatialChanged;
	}


	public void UpdateLoading(WorldGridPos loadPos) => sim.UpdateLoading(loadPos);

	public void SetBlock(WorldGridPos worldPos, Block block)
	{
		Core.SetBlock(worldPos.ToConstruct(Core.Data.GridTransform), block);
	}
	public void SetBlocks(WorldGridPos[] worldPositions, Block[] blocks)
	{
		ConstructGridPos[] constPositions = worldPositions
			.Select(worldPos => worldPos.ToConstruct(Core.Data.GridTransform)).ToArray();
		Core.SetBlocks(constPositions, blocks);
	}
	public bool TryGetBlock(WorldGridPos worldPos, out Block block) => Core.TryGetBlock(worldPos, out block);

	public Vector3I GetRootPos() => Core.Data.GridTransform.WorldPos;

	//Todo: Cash this value
	public Vector3I GetMin() => Core.Data.Bounds.MinPos.ToWorld(Core.Data.GridTransform);
	public Vector3I GetMax() => Core.Data.Bounds.MaxPos.ToWorld(Core.Data.GridTransform);

	private void OnSpatialChanged()
	{
		BoundsChanged?.Invoke(this);
	}
}
