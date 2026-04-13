using System;
using System.Linq;
using Godot;


public partial class Module
{
	public Action<BlockChange[]> OnModuleChanged;

	public int BlockCount { get; private set; }
	public ModuleGridPos MaxPos => new ModuleGridPos(bounds.MaxPos);
	public ModuleGridPos MinPos => new ModuleGridPos(bounds.MinPos);
	public SurfaceCacheController SurfaceCache = new SurfaceCacheController();
	private int moduleSize;
	public Block[] BlocksArrayCopy
	{
		get
		{
			return blocks.ToArray();
		}
	}
	public bool HasBlocks
	{
		get
		{
			return BlockCount > 0;
		}
	}
	private Block[] blocks = [];
	private Vector3IBounds bounds;

	public Module()
	{
		moduleSize = GameSettings.Instance.ModuleSize;

		bounds = new Vector3IBounds(moduleSize);

		int blockCount = (int)Mathf.Pow(moduleSize, 3);
		blocks = new Block[blockCount];
		BlockCount = 0;
	}

	public Block[] GetBlockArray()
	{
		return blocks;
	}

	public Block GetBlock(ModuleGridPos modulePos)
	{
		int index = InModuleToArrayPos(modulePos);
		return blocks[index];
	}

	public bool HasBlock(ModuleGridPos modulePos, out Block block)
	{
		int index = InModuleToArrayPos(modulePos);
		block = default;
		if (blocks.Length > index && index >= 0)
		{
			block = blocks[index];
			return !block.IsEmpty;
		}
		return false;
	}

	public bool HasBlock(ModuleGridPos modulePos)
	{
		Block _;
		return HasBlock(modulePos, out _);
	}

	public void SetBlock(ModuleGridPos modulePos, Block block)
	{
		int index = InModuleToArrayPos(modulePos);
		Block prevBlock = blocks[index];
		BlockChange blockChange = default;
		bool blockChanged = false;

		if (prevBlock != block)
		{
			if (prevBlock.IsEmpty && !block.IsEmpty)
			{
				// Adding a block
				BlockCount++;
				bounds.AddPoint(modulePos.Value, BlockCount);
				SurfaceCache.AddBlock(this, modulePos, block);
				blockChange = new BlockChange(modulePos, BlockChangeAction.PLACE, block);
				blockChanged = true;
			}
			else if (!prevBlock.IsEmpty && block.IsEmpty)
			{
				// Removing a block
				BlockCount--;
				bounds.RemovePoint(modulePos.Value, BlockCount);
				SurfaceCache.RemoveBlock(this, modulePos);
				blockChange = new BlockChange(modulePos, BlockChangeAction.REMOVE, block);
				blockChanged = true;
			}
		}

		if (blockChanged)
		{
			blocks[index] = block;
			OnModuleChanged?.Invoke([blockChange]);
		}
	}

	public void SetBlocks(BlockChange[] blockActionArray)
	{
		TimeTracker.Start("Module Block put", TimeTracker.TrackingType.Average);

		// Apply all block changes
		foreach (var blockChange in blockActionArray)
		{
			Block newBlock = blockChange.Block;
			ModuleGridPos modPos = blockChange.Position;
			int index = InModuleToArrayPos(modPos);
			Block oldBlock = blocks[index];

			if (oldBlock.IsEmpty && !newBlock.IsEmpty)
			{
				// Adding a block
				BlockCount++;
				bounds.AddPoint(modPos, BlockCount);
				blocks[index] = blockChange.Block;
			}
			else if (!oldBlock.IsEmpty && newBlock.IsEmpty)
			{
				// Removing a block
				BlockCount--;
				bounds.RemovePoint(modPos, BlockCount);
				blocks[index] = default;
			}
		}
		TimeTracker.End("Module Block put");

		// Restore surface cache and rebuild it
		TimeTracker.Start("Module Surface Cache generation", TimeTracker.TrackingType.Average);
		SurfaceCache.RebuildModule(this);
		TimeTracker.End("Module Surface Cache generation");

		OnModuleChanged?.Invoke(blockActionArray);
	}

	public int InModuleToArrayPos(ModuleGridPos modulePos)
	{
		return modulePos.Value.X
			 + modulePos.Value.Y * moduleSize
			 + modulePos.Value.Z * moduleSize * moduleSize;
	}

	public ModuleGridPos ArrayToInModulePos(int index)
	{
		int x = index % moduleSize;
		int y = index / moduleSize % moduleSize;
		int z = index / (moduleSize * moduleSize);
		return new(new(x, y, z));
	}

	public bool IsInModule(ConstructGridPos inConstructPos, ModuleLocation moduleLocation)
	{
		Vector3I minModuleWorldPos = moduleLocation.Value * moduleSize;
		Vector3I maxModuleWorldPos = (moduleLocation.Value + Vector3I.One) * moduleSize;
		bool correctX = inConstructPos.Value.X >= minModuleWorldPos.X && inConstructPos.Value.X < maxModuleWorldPos.X;
		bool correctY = inConstructPos.Value.Y >= minModuleWorldPos.Y && inConstructPos.Value.Y < maxModuleWorldPos.Y;
		bool correctZ = inConstructPos.Value.Z >= minModuleWorldPos.Z && inConstructPos.Value.Z < maxModuleWorldPos.Z;
		return correctX && correctY && correctZ;
	}

	public bool IsInModule(ModuleGridPos modulePos)
	{
		return bounds.IsValidPoint(modulePos.Value);
	}

	// Additional utility methods using the generic bounds
	public Vector3I GetBoundsSize()
	{
		return bounds.GetBoundsSize();
	}

	public int GetBoundsVolume()
	{
		return bounds.GetBoundsVolume();
	}

	public bool IsBlockOnBoundary(ModuleGridPos modulePos)
	{
		return bounds.IsPointOnBoundary(modulePos.Value);
	}
}