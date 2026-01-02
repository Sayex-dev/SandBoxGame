using Godot;
using System;
using System.Runtime.InteropServices;

[GlobalClass]
public partial class StructurePlacement : Resource
{
	[Export] public Structure structure;
	[Export] public float gridSize;
	[Export] public bool grounded;

	private int noiseScale = 1000;

	private FastNoiseLite noise = new FastNoiseLite();

	public StructurePlacement()
	{
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
	}

	public Vector3I GetClosest(int seed, Vector3I worldPos, Module module)
	{
		return Vector3I.Down;
	}
}
