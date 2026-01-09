using Godot;
using System.Collections.Generic;

public partial class Module : MeshInstance3D
{
	public int ModuleSize { get; private set; }
	public int BlockCount { get; private set; }
	public bool HasBlocks
	{
		get
		{
			return BlockCount > 0;
		}
	}
	private int[] blocks = [];
	public Dictionary<ModuleGridPos, BlockState> BlockStates { get; private set; } = new();
	private Material moduleMaterial;
	public Module(int moduleSize, Material moduleMaterial)
	{
		ModuleSize = moduleSize;
		this.moduleMaterial = moduleMaterial;

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
		if (blocks.Length > index)
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
			BlockCount += prevBlockId == -1 ? 1 : -1;
		}

		blocks[index] = blockId;
	}

	public BlockState GetBlockState(ModuleGridPos modulePos)
	{
		return BlockStates.GetValueOrDefault(modulePos);
	}

	public void SetBlockState(ModuleGridPos modulePos, BlockState blockState)
	{
		BlockStates[modulePos] = blockState;
	}

	public bool HasBlockState(ModuleGridPos modulePos)
	{
		return BlockStates.ContainsKey(modulePos);
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

	public bool IsInModule(ConstructGridPos inConstructPos)
	{
		bool correctX = inConstructPos.Value.X >= 0 && inConstructPos.Value.X < ModuleSize;
		bool correctY = inConstructPos.Value.Y >= 0 && inConstructPos.Value.Y < ModuleSize;
		bool correctZ = inConstructPos.Value.Z >= 0 && inConstructPos.Value.Z < ModuleSize;
		return correctX && correctY && correctZ;
	}

	public void BuildMesh(BlockStore blockStore)
	{
		if (!HasBlocks)
		{
			return;
		}

		Mesh = ModuleMeshGenerator.BuildModuleMesh(
			this,
			moduleMaterial,
			blockStore
		);
	}
}