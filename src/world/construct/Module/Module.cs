using System.Linq;
using Godot;


public partial class Module
{
	public int ModuleSize { get; private set; }
	public int BlockCount { get; private set; }
	public ModuleGridPos MaxPos => new ModuleGridPos(bounds.MaxPos);
	public ModuleGridPos MinPos => new ModuleGridPos(bounds.MinPos);
	public ExposedModuleSurfaceCache SurfaceCache;
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

	public Module(int moduleSize, ExposedModuleSurfaceCache surfaceCache = null)
	{
		ModuleSize = moduleSize;
		SurfaceCache = surfaceCache != null ? surfaceCache : new();

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

		if (prevBlock != block)
		{
			if (prevBlock.IsEmpty && !block.IsEmpty)
			{
				// Adding a block
				BlockCount++;
				bounds.AddPoint(modulePos.Value, BlockCount);
				SurfaceCache.AddBlock(modulePos);
			}
			else if (!prevBlock.IsEmpty && block.IsEmpty)
			{
				// Removing a block
				BlockCount--;
				bounds.RemovePoint(modulePos.Value, BlockCount);
				SurfaceCache.RemoveBlock(modulePos);
			}
		}

		blocks[index] = block;
	}

	public void SetAllBlocks(BlockChange[] blockActionArray)
	{
		TimeTracker.Start("Module Block put", TimeTracker.TrackingType.Average);

		// Apply all block changes
		for (int i = 0; i < blockActionArray.Length; i++)
		{
			if (blockActionArray[i].Action == BlockChangeAction.KEEP_PREVIOUS)
			{
				continue;
			}

			Block newBlock = blockActionArray[i].Block;
			Block oldBlock = blocks[i];

			ModuleGridPos modPos = ArrayToInModulePos(i);
			if (oldBlock.IsEmpty && !newBlock.IsEmpty)
			{
				// Adding a block
				BlockCount++;
				bounds.AddPoint(modPos, BlockCount);
			}
			else if (!oldBlock.IsEmpty && newBlock.IsEmpty)
			{
				// Removing a block
				BlockCount--;
				bounds.RemovePoint(modPos, BlockCount);
			}

			blocks[i] = blockActionArray[i].Block;
		}
		TimeTracker.End("Module Block put");

		// Restore surface cache and rebuild it
		TimeTracker.Start("Module Surface Cache generation", TimeTracker.TrackingType.Average);
		SurfaceCache.RebuildModule(this);
		TimeTracker.End("Module Surface Cache generation");
	}

	public int InModuleToArrayPos(ModuleGridPos modulePos)
	{
		return modulePos.Value.X
			 + modulePos.Value.Y * ModuleSize
			 + modulePos.Value.Z * ModuleSize * ModuleSize;
	}

	public ModuleGridPos ArrayToInModulePos(int index)
	{
		int x = index % ModuleSize;
		int y = index / ModuleSize % ModuleSize;
		int z = index / (ModuleSize * ModuleSize);
		return new(new(x, y, z));
	}

	public bool IsInModule(ConstructGridPos inConstructPos, ModuleLocation moduleLocation)
	{
		Vector3I minModuleWorldPos = moduleLocation.Value * ModuleSize;
		Vector3I maxModuleWorldPos = (moduleLocation.Value + Vector3I.One) * ModuleSize;
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