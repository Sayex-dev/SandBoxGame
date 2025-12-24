using Godot;
using System;

[GlobalClass]
public partial class NoiseLayer : Resource
{
	[Export] public float NoiseScale = 1f;
	[Export] public float NoiseHeight = 10f;
	[Export] public int NoiseHeightOffset = 0;
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Simplex;
}
