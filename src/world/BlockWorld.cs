using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;


public partial class BlockWorld : Node3D, IWorldCollisionQuery
{
	private int seed;
	private int moduleSize;
	private BlockStore blockStore;
	private Material moduleMaterial;
	private AbilityManager abilityManager;
	private ExpandingOctTree<Construct> constructs;

	// Streaming loaders for global constructs (keyed by construct)
	private readonly Dictionary<Construct, ConstructStreamingLoader> streamingLoaders = new();

	public BlockWorld(
		int seed,
		int moduleSize,
		BlockStore blockStore,
		Material moduleMaterial,
		AbilityManager abilityManager
	)
	{
		this.seed = seed;
		this.moduleSize = moduleSize;
		this.blockStore = blockStore;
		this.moduleMaterial = moduleMaterial;
		this.abilityManager = abilityManager;

		constructs = new ExpandingOctTree<Construct>(32, Vector3I.Zero);
	}

	public void SetBlock(Construct construct, WorldGridPos worldPos, int blockId)
	{
		construct.SetBlock(worldPos, blockId);
	}

	public int GetBlock(WorldGridPos worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			int blockId;
			if (construct.TryGetBlock(worldPos, out blockId))
			{
				return blockId;
			}
		}
		return -1;
	}

	public Construct HasBlock(WorldGridPos worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			if (construct.TryGetBlock(worldPos, out _))
			{
				return construct;
			}
		}
		return null;
	}

	public async Task AddConstruct(Construct construct)
	{
		constructs.Insert(construct);
		AddChild(construct);

		// One-time load for finite constructs
		var generator = construct.ConstructGeneratorSettings.CreateConstructGenerator(moduleSize, seed);
		var worldPos = new WorldGridPos(construct.Data.Transform.WorldPos.Value);
		int loadDistance = 1; // Enough to cover the construct's required modules
		await ConstructOneTimeLoader.LoadAll(
			construct.Data, construct.ModuleBuilder, construct.Visuals,
			generator, worldPos, loadDistance);
	}

	/// <summary>
	/// Adds a global construct and creates a streaming loader for it.
	/// </summary>
	public void AddGlobalConstruct(Construct construct)
	{
		constructs.InsertGlobal(construct);
		AddChild(construct);

		// Create a streaming loader for this global construct
		var generator = construct.ConstructGeneratorSettings.CreateConstructGenerator(moduleSize, seed);
		var loader = new ConstructStreamingLoader(
			construct.Data, construct.ModuleBuilder, construct.Visuals, generator);
		streamingLoaders[construct] = loader;
	}

	public bool HasBlockAt(WorldGridPos worldPos)
	{
		return HasBlock(worldPos) != null;
	}

	public List<Construct> GetConstructsInArea(WorldGridPos min, WorldGridPos max)
	{
		return constructs.QueryBox(min.Value, max.Value);
	}

	/// <summary>
	/// Updates streaming loading for all global constructs in range.
	/// Finite constructs are already fully loaded and are skipped.
	/// </summary>
	public async Task UpdateConstructLoading(WorldGridPos worldPos, int renderDistance, int simulationDistance)
	{
		foreach (var kvp in streamingLoaders)
		{
			await kvp.Value.UpdateLoading(worldPos, renderDistance, simulationDistance);
		}
	}
}
