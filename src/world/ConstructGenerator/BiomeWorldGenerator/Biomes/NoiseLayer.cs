using Godot;
using System;
using System.Runtime.InteropServices;

[GlobalClass]
public partial class NoiseLayer : Resource
{
	[Export] public float NoiseScale = 1f;
	[Export] public float NoiseHeight = 10f;
	[Export] public int NoiseHeightOffset = 0;
	[Export] public float HeightPow = 1;
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Simplex;

	public float GetNoiseHeight2D(Vector2 pos, FastNoiseLite noise)
	{
		noise.NoiseType = NoiseType;
		return (float)Math.Pow(noise.GetNoise2Dv(pos * NoiseScale) * NoiseHeight, HeightPow) + NoiseHeightOffset;
	}
}
