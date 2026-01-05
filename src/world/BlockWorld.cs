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
	private ExpandingOctTree<Construct> constructs;
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

		constructs = new ExpandingOctTree<Construct>(32, Vector3I.Zero);
	}

	public void AddConstruct(Construct construct)
	{
		constructs.Insert(construct);
		AddChild(construct);
	}

	public void AddGlobalConstruct(Construct construct)
	{
		constructs.InsertGlobal(construct);
		AddChild(construct);
	}

	public void UpdateConstructLoading(Vector3I worldPos, Vector3I renderDistance)
	{
		Vector3I minPos = worldPos - renderDistance * moduleSize;
		Vector3I maxPos = worldPos + renderDistance * moduleSize;

		List<Construct> nearConstructs = constructs.QueryBox(minPos, maxPos);
		foreach (Construct construct in nearConstructs)
		{
			construct.LoadPosition(worldPos, renderDistance);
		}
	}
}