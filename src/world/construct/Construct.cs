using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Construct : Node3D, IHaveBounds
{
	public ConstructGridTransform ConstructTransform { get; private set; }
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
		ConstructTransform = FindChildOfType<ConstructGridTransform>();

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

	public void SetBlocks(WorldGridPos[] worldPositions, int[] blockIds)
	{
		HashSet<ModuleLocation> moduleLocations = [];
		for (int i = 0; i < worldPositions.Length; i++)
		{
			WorldGridPos worldPos = worldPositions[i];
			int blockId = blockIds[i];
			ModuleLocation moduleLoc = worldPos.ToModuleLocation(ConstructTransform, Modules.ModuleSize);
			moduleLocations.Add(moduleLoc);
			SetBlockInternal(worldPos, blockId);
		}

		foreach (var moduleLoc in moduleLocations)
		{
			UpdateModuleMesh(moduleLoc).FireAndForget();
		}
	}

	public void SetBlock(WorldGridPos worldPos, int blockId)
	{
		SetBlockInternal(worldPos, blockId);
		ModuleLocation moduleLoc = worldPos.ToModuleLocation(ConstructTransform, Modules.ModuleSize);
		UpdateModuleMesh(moduleLoc).FireAndForget();
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

	private void SetBlockInternal(WorldGridPos worldPos, int blockId)
	{
		ConstructGridPos conPos = worldPos.ToConstruct(ConstructTransform);

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

	private async Task UpdateModuleMesh(ModuleLocation moduleLoc)
	{
		Module module;
		if (!Modules.TryGet(moduleLoc, out module)) return;


		var context = new ModuleMeshGenerateContext(
			module,
			moduleLoc,
			blockStore,
			moduleMaterial
		);
		var mesh = await moduleBuilder.GenerateModuleMesh(context);
		visuals.RemoveModule(moduleLoc);
	}

	public async Task LoadAround(WorldGridPos worldPos, Vector3I renderDistance)
	{
		var context = new ModuleLoadContext(
			Modules.ModuleSize,
			blockStore,
			moduleMaterial,
			constructGenerator
		);
		var tasks = await moduleBuilder.GenerateModulesAround(worldPos, renderDistance, ConstructTransform, Modules, context);

		foreach (Task<GenerateModulesResponse> task in tasks)
		{
			var response = await task;
			foreach (var kvp in response.GeneratedModules)
			{
				ModuleLocation moduleLocation = kvp.Key;
				Module module = kvp.Value;
				Mesh mesh = response.Meshes[moduleLocation];

				// Update bounds
				bounds.AddPosition(module.MinPos.ToConstruct(moduleLocation, module.ModuleSize));
				bounds.AddPosition(module.MaxPos.ToConstruct(moduleLocation, module.ModuleSize));

				// Update modules
				Modules.Add(moduleLocation, module);

				// Update visuals
				visuals.AddModule(moduleLocation, mesh);
			}
		}
	}

	private void UpdateBoundsOnRemove(ConstructGridPos pos)
	{
		if (!bounds.IsOnBounds(pos)) return;

		// Rebuild bounds
		bounds.Clear();
		foreach (var kvp in Modules.Modules)
		{
			var moduleLocation = kvp.Key;
			var module = kvp.Value;

			bounds.AddPosition(module.MinPos.ToConstruct(moduleLocation, module.ModuleSize));
		}
	}
}
