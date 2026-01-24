using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public partial class Construct : Node3D, IHaveBounds
{
	public ConstructGridTransform ConstructTransform { get; private set; }
	public ExposedSurfaceCache exposedSurfaceCache { get; private set; }
	public ConstructModuleController Modules { get; private set; }
	private ConstructVisualsController visuals;
	private ConstructMotionController motion;
	private ConstructBoundsController bounds;
	private ConstructGenerator constructGenerator;
	private ConstructModuleBuilder moduleBuilder;

	private BlockStore blockStore;
	private Material moduleMaterial;
	private bool isStatic;

	public void Initialize(
		ConstructVisualsController visuals,
		ConstructMotionController motion,
		ConstructGridTransform gridTransform
	)
	{
		this.visuals = visuals;
		this.motion = motion;
		this.ConstructTransform = gridTransform;

		Position = gridTransform.WorldPos.Value;
		Rotation = motion.Rotation;

		SetPhysicsProcess(true);
	}
	public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (motion != null)
		{
			motion.Update(delta, ConstructTransform.WorldPos, ConstructTransform.YRotation);
			Position = motion.Position;
			Rotation = motion.Rotation;
		}
	}

	public async Task LoadAround(WorldGridPos worldPos, Vector3I renderDistance)
	{
		var context = new ModuleLoadContext(
			Modules.ModuleSize,
			blockStore,
			moduleMaterial,
			exposedSurfaceCache,
			constructGenerator
		);
		var tasks = await moduleBuilder.LoadAroundPosition(worldPos, renderDistance, ConstructTransform, Modules, context);

		foreach (Task<ModuleGenerationResponse> task in tasks)
		{
			var response = await task;
			bounds.CombineWith(response.bounds);

			foreach (var kvp in response.GeneratedModules)
			{

			}
		}
	}

	public void SetBlocks(WorldGridPos[] worldPositions, int[] blockIds)
	{
		for (int i = 0; i < worldPositions.Length; i++)
		{
			WorldGridPos worldPos = worldPositions[i];
			int blockId = blockIds[i];

			ConstructGridPos conPos = worldPos.ToConstruct(ConstructTransform);

			exposedSurfaceCache.AddBlock(conPos);
			Modules.SetBlock(conPos, blockId);

			if (blockId == -1)
			{
				UpdateBoundsOnRemove(conPos);
			}
			else
			{
				bounds.AddPosition(conPos);
			}
		}
		UpdateModules().FireAndForget();
	}

	public void SetBlock(WorldGridPos worldPos, int blockId)
	{
		ConstructGridPos conPos = worldPos.ToConstruct(ConstructTransform);
		exposedSurfaceCache.AddBlock(conPos);
		bounds.AddPosition(conPos);
		Modules.SetBlock(conPos, blockId);
		UpdateModules().FireAndForget();
	}

	public bool TryGetBlock(WorldGridPos worldPos, out int blockId)
	{
		ConstructGridPos conPos = worldPos.ToConstruct(ConstructTransform);
		return Modules.TryGetBlock(conPos, out blockId);
	}

	public Vector3I GetRootPos()
	{
		return ConstructTransform.WorldPos;
	}

	public Vector3I GetMin()
	{
		return bounds.MinPos.ToWorld(ConstructTransform);
	}

	public Vector3I GetMax()
	{
		return bounds.MaxPos.ToWorld(ConstructTransform);
	}

	private async Task UpdateModules()
	{
		var context = new ModuleLoadContext(
			Modules.ModuleSize,
			blockStore,
			moduleMaterial,
			exposedSurfaceCache,
			constructGenerator
		);
		var tasks = await moduleBuilder.LoadAroundPosition(worldPos, renderDistance, ConstructTransform, Modules, context);

		foreach (Task<ModuleGenerationResponse> task in tasks)
		{
			var response = await task;
			bounds.CombineWith(response.bounds);

			foreach (var kvp in response.GeneratedModules)
			{

			}
		}
	}

	private void UpdateBoundsOnRemove(ConstructGridPos pos)
	{
		if (!bounds.IsOnBounds(pos)) return;

		bounds.Clear();
		foreach (var kvp in Modules.Modules)
		{
			var moduleLocation = kvp.Key;
			var module = kvp.Value;

			bounds.AddPosition(module.MinPos.ToConstruct(moduleLocation, module.ModuleSize));
		}
	}
}
