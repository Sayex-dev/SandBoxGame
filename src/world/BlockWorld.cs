using Godot;
using System.Collections.Generic;


public partial class BlockWorld : Node3D
{
	private int seed;
	private int moduleSize;
	private BlockStore blockStore;
	private ConstructGenerator worldGen;
	private Material moduleMaterial;
	private AbilityManager abilityManager;
	private Dictionary<Vector3I, Construct> constructs = [];
	public BlockWorld(
		int seed,
		int moduleSize,
		BlockStore blockStore,
		ConstructGenerator worldGen,
		Material moduleMaterial,
		AbilityManager abilityManager
	)
	{
		this.seed = seed;
		this.moduleSize = moduleSize;
		this.blockStore = blockStore;
		this.worldGen = worldGen;
		this.moduleMaterial = moduleMaterial;
		this.abilityManager = abilityManager;
	}

	public void UpdateConstructLoading(Vector3I worldPos, Vector3I renderDistance)
	{

	}
}