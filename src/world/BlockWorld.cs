using Godot;
using System.Collections.Generic;


public partial class BlockWorld : Node3D
{
	private int seed;
	private int moduleSize;
	private BlockStore blockStore;
	private Material moduleMaterial;
	private AbilityManager abilityManager;
	private ExpandingOctTree<Construct> constructs;
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
			int blockId = construct.GetBlock(worldPos);
			if (blockId != -1)
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
			int blockId = construct.GetBlock(worldPos);
			if (blockId != -1)
			{
				return construct;
			}
		}
		return null;
	}

	public void SetBlockState(Construct construct, WorldGridPos worldPos, BlockState blockState)
	{
		construct.SetBlockState(worldPos, blockState);
	}

	public BlockState GetBlockState(WorldGridPos worldPos)
	{
		List<Construct> nearConstructs = constructs.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			BlockState blockState = construct.GetBlockState(worldPos);
			if (blockState != null) return blockState;
		}
		return null;
	}

	public Construct HasBlockState(WorldGridPos worldPos)
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

	public List<Construct> GetConstructsInArea(WorldGridPos min, WorldGridPos max)
	{
		return constructs.QueryBox(min.Value, max.Value);
	}

	public void UpdateConstructLoading(WorldGridPos worldPos, Vector3I renderDistance)
	{
		Vector3I minPos = worldPos.Value - renderDistance * moduleSize;
		Vector3I maxPos = worldPos.Value + renderDistance * moduleSize;

		List<Construct> nearConstructs = constructs.QueryBox(minPos, maxPos);
		foreach (Construct construct in nearConstructs)
		{
			construct.LoadPosition(worldPos, renderDistance);
		}
	}
}