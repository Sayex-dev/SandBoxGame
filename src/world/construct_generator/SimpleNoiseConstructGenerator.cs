using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class SimpleNoiseConstructGenerator : ConstructGenerator
{
	[Export] public Vector3I GenOffset { get; set; }
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Simplex;
	[Export] public float NoiseScale { get; set; } = 1;
	[Export] public float HeightScale { get; set; } = 10;

	private FastNoiseLite _noise = new FastNoiseLite();

	public SimpleNoiseConstructGenerator()
	{
		_noise.NoiseType = NoiseType;
	}

	public override GenerationResponse GenerateModules(
		Vector3I moduleLocation,
		Material moduleMat,
		HashSet<Vector3I> prevLoaded = null
	)
	{

		var module = new Module(moduleSize, moduleMat);
		SetGround(module, moduleLocation);
		return new GenerationResponse
		{
			generatedAllModules = false,
			generatedModules = new Dictionary<Vector3I, Module>
			{
				{moduleLocation, module}
			}
		};
	}

	private void SetGround(Module module, Vector3I moduleLocation)
	{
		var moduleSize = module.ModuleSize;

		for (int x = 0; x < moduleSize; x++)
		{
			for (int z = 0; z < moduleSize; z++)
			{
				float xPos = (moduleLocation.X * moduleSize + x + GenOffset.X) * NoiseScale;
				float zPos = (moduleLocation.Z * moduleSize + z + GenOffset.Z) * NoiseScale;

				int noiseHeight = (int)(_noise.GetNoise2D(xPos, zPos) * HeightScale) + GenOffset.Y;

				int maxY = Mathf.Min(noiseHeight - moduleLocation.Y * moduleSize, moduleSize);

				for (int y = 0; y < maxY; y++)
				{
					var inModulePos = new Vector3I(x, y, z);
					module.SetBlock(inModulePos, 1);
				}
			}
		}
	}

	public override void SetSeed(int seed)
	{
		_noise.Seed = seed;
	}

	public override bool IsModuleNeeded(Vector3I chunkLocation)
	{
		return chunkLocation.Y <= (int)(HeightScale / moduleSize);
	}
}
