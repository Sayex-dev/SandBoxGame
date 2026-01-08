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

	public void SetBlock(Construct construct, Vector3I worldPos, int blockId)
	{
		construct.SetBlock(worldPos, blockId);
	}

	public int GetBlock(Vector3I worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			int blockId = construct.GetBlock(worldPos);
			if (blockId != -1)
			{
				return blockId;
			}
		}
		return -1;
	}

	public Construct HasBlock(Vector3I worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			int blockId = construct.GetBlock(worldPos);
			if (blockId != -1)
			{
				return construct;
			}
		}
		return null;
	}

	public void SetBlockState(Construct construct, Vector3I worldPos, BlockState blockState)
	{
		construct.SetBlockState(worldPos, blockState);
	}

	public BlockState GetBlockState(Vector3I worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			BlockState blockState = construct.GetBlockState(worldPos);
			if (blockState != null) return blockState;
		}
		return null;
	}

	public Construct HasBlockState(Vector3I worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			BlockState blockState = construct.GetBlockState(worldPos);
			if (blockState != null) return construct;
		}
		return null;
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

	public List<Construct> GetConstructsInArea(Vector3I min, Vector3I max)
	{
		return constructs.QueryBox(min, max);
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