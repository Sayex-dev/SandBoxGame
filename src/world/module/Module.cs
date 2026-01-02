using Godot;
using System.Collections.Generic;

public partial class Module : MeshInstance3D, IBlockContainer
{
	public int ModuleSize { get; private set; }
	private int[] blocks = [];
	public Dictionary<Vector3I, BlockState> BlockStates { get; private set; } = new();
	private Material moduleMaterial;
	public Module(int moduleSize, Material moduleMaterial)
	{
		ModuleSize = moduleSize;
		this.moduleMaterial = moduleMaterial;

		int blockCount = (int)Mathf.Pow(moduleSize, 3);
		blocks = new int[blockCount];
		for (int i = 0; i < blockCount; i++)
			blocks[i] = -1;
	}

	public int GetBlock(Vector3I modulePos)
	{
		int index = InModuleToArrayPos(modulePos);
		return blocks[index];
	}

	public bool HasBlock(Vector3I localPos)
	{
		int index = InModuleToArrayPos(localPos);
		return blocks.Length > index && blocks[index] != -1;
	}

	public void SetBlock(Vector3I modulePos, int blockId)
	{
		int index = InModuleToArrayPos(modulePos);
		blocks[index] = blockId;
	}

	public BlockState GetBlockState(Vector3I modulePos)
	{
		return BlockStates[modulePos];
	}

	public void SetBlockState(Vector3I modulePos, BlockState blockState)
	{
		BlockStates[modulePos] = blockState;
	}

	public bool HasBlockState(Vector3I modulePos)
	{
		return BlockStates.ContainsKey(modulePos);
	}

	public static Vector3I WorldToInModulePos(Vector3I worldPos, int moduleSize, Vector3I moduleLocation)
	{
		return worldPos - (moduleSize * moduleLocation);
	}

	public static Vector3I WrapToModule(Vector3I pos, int moduleSize)
	{
		return new Vector3I(
			Mathf.PosMod(pos.X, moduleSize),
			Mathf.PosMod(pos.Y, moduleSize),
			Mathf.PosMod(pos.Z, moduleSize)
		);
	}

	public static Vector3I InModuleToWorldPos(Vector3I moduleLocation, Vector3I moduleSize, Vector3I modulePos)
	{
		return (moduleLocation * moduleSize) + modulePos;
	}

	public static Vector3I WorldToModuleLocation(Vector3I modulePos, int moduleSize)
	{
		return new Vector3I(
			Mathf.FloorToInt((float)modulePos.X / moduleSize),
			Mathf.FloorToInt((float)modulePos.Y / moduleSize),
			Mathf.FloorToInt((float)modulePos.Z / moduleSize)
		);
	}

	public int InModuleToArrayPos(Vector3I modulePos)
	{
		return modulePos.X
			 + modulePos.Y * ModuleSize
			 + modulePos.Z * ModuleSize * ModuleSize;
	}

	public Vector3I ArrayToInModulePos(int index)
	{
		int x = index % ModuleSize;
		int y = index / ModuleSize % ModuleSize;
		int z = index / (ModuleSize * ModuleSize);
		return new Vector3I(x, y, z);
	}

	public bool IsInModule(Vector3I modulePos)
	{
		bool correctX = modulePos.X >= 0 && modulePos.X < ModuleSize;
		bool correctY = modulePos.Y >= 0 && modulePos.Y < ModuleSize;
		bool correctZ = modulePos.Z >= 0 && modulePos.Z < ModuleSize;
		return correctX && correctY && correctZ;
	}

	public void BuildMesh(
		BlockStore blockStore)
	{
		Mesh = ModuleMeshGenerator.BuildModuleMesh(
			this,
			moduleMaterial,
			blockStore
		);
	}
}