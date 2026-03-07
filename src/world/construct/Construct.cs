using Godot;
using System;

[GlobalClass]
public partial class Construct : Node3D, IOctTreeObject
{
	public event Action<IOctTreeObject> BoundsChanged;

	[Export] public bool IsGlobal { get; private set; }
	[Export] public ConstructGeneratorSettings ConstructGeneratorSettings { get; private set; }
	[Export] private SecondOrderDynamicsSettings rotSodSettings;
	[Export] private SecondOrderDynamicsSettings moveSodSettings;

	public ConstructData Data { get; private set; }
	public ConstructBlockService Blocks { get; private set; }
	public ConstructVisualsController Visuals { get; private set; }
	public ConstructModuleBuilder ModuleBuilder { get; private set; }

	private ConstructPhysicsController physics;
	private ConstructVisualMotionController visualMotion;
	private ConstructMotionController motionController;

	public void Initialize(int moduleSize, BlockStore blockStore, Material moduleMaterial, IWorldQuery collisionQuery)
	{
		var transform = new ConstructTransform((Vector3I)Position);
		var modules = new ConstructModules(moduleSize);
		var bounds = new ConstructBounds();

		Data = new ConstructData(transform, modules, bounds, blockStore, moduleMaterial);

		Data.Transform.Changed += OnSpatialChanged;
		Data.Bounds.Changed += OnSpatialChanged;

		motionController = new ConstructMotionController(Data, collisionQuery);
		physics = new ConstructPhysicsController(Data, motionController, Position, IsGlobal);

		SecondOrderDynamics<float> rotSod = rotSodSettings.GetInstance(0);
		SecondOrderDynamics<Vector3> moveSod = moveSodSettings.GetInstance(Position);
		visualMotion = new ConstructVisualMotionController(Data, moveSod, rotSod);

		Visuals = new ConstructVisualsController(moduleSize, this);
		ModuleBuilder = new ConstructModuleBuilder();

		Blocks = new ConstructBlockService(Data, ModuleBuilder, Visuals);

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
