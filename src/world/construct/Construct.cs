using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass]
public partial class Construct : Node3D, IHaveBounds
{
	[Export] public bool IsGlobal { get; private set; }
	[Export] private ConstructGeneratorSettings constructGeneratorSettings;
	[Export] private SecondOrderDynamicsSettings rotSodSettings;
	[Export] private SecondOrderDynamicsSettings moveSodSettings;

	public ConstructModuleController Modules { get; private set; }
	public ConstructTransform ConstructTransform { get; private set; }
	public ConstructPhysicsController PhysicsController { get; private set; }

	private ConstructGenerator constructGenerator;
	private ConstructMotionController visualMotion;
	private ConstructVisualsController visuals;
	private ConstructBoundsController bounds;
	private ConstructModuleBuilder moduleBuilder;
	private BlockStore blockStore;
	private Material moduleMaterial;
	private bool loadComplete = true;

	public void InitializePrebuilt(int moduleSize, int seed, BlockStore blockStore, Material moduleMaterial)
	{
		SecondOrderDynamics<float> rotSod = rotSodSettings.GetInstance(0);
		SecondOrderDynamics<Vector3> moveSod = moveSodSettings.GetInstance(Position);

		Initialize(
			new ConstructTransform((Vector3I)Position),
			new ConstructModuleController(moduleSize),
			constructGeneratorSettings.CreateConstructGenerator(moduleSize, seed),
			new ConstructMotionController(moveSod, rotSod, Position, Rotation),
			new ConstructVisualsController(moduleSize, this),
			new ConstructBoundsController(),
			new ConstructModuleBuilder(),
			new ConstructPhysicsController(Position, IsGlobal),
			blockStore,
			moduleMaterial
		);
	}

	public void Initialize(
		ConstructTransform constructTransform,
		ConstructModuleController modules,
		ConstructGenerator constructGenerator,
		ConstructMotionController motion,
		ConstructVisualsController visuals,
		ConstructBoundsController bounds,
		ConstructModuleBuilder moduleBuilder,
		ConstructPhysicsController physicsController,
		BlockStore blockStore,
		Material moduleMaterial
	)
	{
		this.ConstructTransform = constructTransform;
		this.Modules = modules;
		this.constructGenerator = constructGenerator;
		this.visualMotion = motion;
		this.visuals = visuals;
		this.bounds = bounds;
		this.moduleBuilder = moduleBuilder;
		this.PhysicsController = physicsController;
		this.blockStore = blockStore;
		this.moduleMaterial = moduleMaterial;

		Position = ConstructTransform.WorldPos.Value;
		Rotation = motion.Rotation;

		SetPhysicsProcess(true);
	}

	private T FindChildOfType<T>()
	{
		throw new NotImplementedException();
	}

	public override void _Ready()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (visualMotion != null)
		{
			visualMotion.Update(delta, ConstructTransform.WorldPos, ConstructTransform.YRotation);
			Position = visualMotion.Position;
			Rotation = visualMotion.Rotation;
		}

		PhysicsController.Update(delta, ConstructTransform);
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
			bounds.RemovePosition(conPos, Modules.Modules);
		}
		else
		{
			bounds.AddPosition(conPos);
		}
	}

	private async Task UpdateModuleMesh(ModuleLocation moduleLoc)
	{
		Module module;
		if (!Modules.TryGet(moduleLoc, out module))
			return;


		var context = new ModuleMeshGenerateContext(
			module,
			moduleLoc,
			blockStore,
			moduleMaterial
		);
		var mesh = await moduleBuilder.GenerateModuleMesh(context);
		visuals.RemoveModule(moduleLoc);
		visuals.AddModule(moduleLoc, mesh);
	}

	public async Task UpdateLoading(WorldGridPos worldPos, int renderDistance, int simulationDistance)
	{
		loadComplete = false;
		await LoadAround(worldPos, simulationDistance);
		loadComplete = true;

		// Lazy loading of modules
		//for (int i = simulationDistance; i < renderDistance; i++)
		//{
		//	if (!loadComplete)
		//		return;
		//	await LoadAround(worldPos, i);
		//}
	}

	public async Task LoadAround(WorldGridPos worldPos, int loadDistance)
	{
		var context = new ModuleLoadContext(
			Modules.ModuleSize,
			blockStore,
			moduleMaterial,
			constructGenerator
		);
		var generationResponse = moduleBuilder.GenerateModulesAround(worldPos, loadDistance, ConstructTransform, Modules, context);

		// Load new modules
		foreach (Task<GenerateModulesResponse> task in generationResponse.GenerationTaskHandles)
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

		// Unload modules that are out of range
		bool needsBoundsRebuild = false;
		foreach (ModuleLocation moduleLocation in generationResponse.ToUnload)
		{
			// Remove from modules
			if (Modules.Remove(moduleLocation, out Module module))
			{
				// Update bounds if necessary
				if (module.HasBlocks)
				{
					ConstructGridPos minPos = module.MinPos.ToConstruct(moduleLocation, module.ModuleSize);
					ConstructGridPos maxPos = module.MaxPos.ToConstruct(moduleLocation, module.ModuleSize);

					if (bounds.IsOnBounds(minPos) || bounds.IsOnBounds(maxPos))
					{
						needsBoundsRebuild = true;
					}
				}
			}

			// Remove visuals
			visuals.RemoveModule(moduleLocation);
		}

		if (needsBoundsRebuild)
		{
			// Rebuild bounds since we removed a module on the boundary
			bounds.Clear();
			foreach (var kvp in Modules.Modules)
			{
				var remainingModule = kvp.Value;
				var remainingLocation = kvp.Key;
				if (remainingModule.HasBlocks)
				{
					bounds.AddPosition(remainingModule.MinPos.ToConstruct(remainingLocation, remainingModule.ModuleSize));
					bounds.AddPosition(remainingModule.MaxPos.ToConstruct(remainingLocation, remainingModule.ModuleSize));
				}
			}
		}
	}
}
