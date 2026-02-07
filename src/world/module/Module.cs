using System.Collections.Generic;
using Godot;


public partial class Module
{
	public int ModuleSize { get; private set; }
	public int BlockCount { get; private set; }
	public ModuleGridPos MaxPos => new ModuleGridPos(bounds.MaxPos);
	public ModuleGridPos MinPos => new ModuleGridPos(bounds.MinPos);
	public ExposedModuleSurfaceCache SurfaceCache;
	public bool HasBlocks
	{
		get
		{
			return BlockCount > 0;
		}
	}
	private int[] blocks = [];
	private Vector3IBounds bounds;

	public Module(int moduleSize, ExposedModuleSurfaceCache surfaceCache = null)
	{
		ModuleSize = moduleSize;
		SurfaceCache = surfaceCache != null ? surfaceCache : new();

		bounds = new Vector3IBounds(moduleSize);

		int blockCount = (int)Mathf.Pow(moduleSize, 3);
		blocks = new int[blockCount];
		for (int i = 0; i < blockCount; i++)
			blocks[i] = -1;

		BlockCount = 0;
	}

	public int[] GetBlockArray()
	{
		return blocks;
	}

	public int GetBlock(ModuleGridPos modulePos)
	{
		int index = InModuleToArrayPos(modulePos);
		return blocks[index];
	}

	public bool HasBlock(ModuleGridPos modulePos, out int blockId)
	{
		int index = InModuleToArrayPos(modulePos);
		blockId = -1;
		if (blocks.Length > index && index >= 0)
		{
			blockId = blocks[index];
			return blockId != -1;
		}
		return false;
	}

	public bool HasBlock(ModuleGridPos modulePos)
	{
		int _;
		return HasBlock(modulePos, out _);
	}

	public void SetBlock(ModuleGridPos modulePos, int blockId)
	{
		int index = InModuleToArrayPos(modulePos);
		int prevBlockId = blocks[index];

		if (prevBlockId != blockId)
		{
			if (prevBlockId == -1 && blockId != -1)
			{
				// Adding a block
				BlockCount++;
				bounds.AddPoint(modulePos.Value, BlockCount);
				SurfaceCache.AddBlock(modulePos);
			}
			else if (prevBlockId != -1 && blockId == -1)
			{
				// Removing a block
				BlockCount--;
				bounds.RemovePoint(modulePos.Value, BlockCount);
				SurfaceCache.RemoveBlock(modulePos);
			}
		}

		blocks[index] = blockId;
	}

	public void SetAllBlocks(int[] blockArray)
	{
		TimeTracker.Start("Module Block put", TimeTracker.TrackingType.Average);

		// Apply all block changes
		for (int i = 0; i < blockArray.Length; i++)
		{
			int newBlockId = blockArray[i];
			int oldBlockId = blocks[i];

			if (newBlockId == -2)
			{
				continue;
			}

			ModuleGridPos modPos = ArrayToInModulePos(i);
			if (oldBlockId == -1 && newBlockId != -1)
			{
				// Adding a block
				BlockCount++;
				bounds.AddPoint(modPos, BlockCount);
			}
			else if (oldBlockId != -1 && newBlockId == -1)
			{
				// Removing a block
				BlockCount--;
				bounds.AddPoint(modPos, BlockCount);
			}

			blocks[i] = blockArray[i];
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