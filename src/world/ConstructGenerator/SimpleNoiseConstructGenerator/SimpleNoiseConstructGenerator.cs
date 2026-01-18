using System.Collections.Generic;
using Godot;


public partial class SimpleNoiseConstructGenerator : ConstructGenerator
{
	private float noiseScale;
	private float heightScale;
	private Vector3I genOffset;

	private FastNoiseLite noise = new FastNoiseLite();

	public SimpleNoiseConstructGenerator(
		int moduleSize,
		int seed,
		float noiseScale,
		float heightScale,
		Vector3I genOffset,
		FastNoiseLite.NoiseTypeEnum noiseType
	) : base(moduleSize, seed)
	{
		this.genOffset = genOffset;
		this.noiseScale = noiseScale;
		this.heightScale = heightScale;
		noise.NoiseType = noiseType;
	}

	public override GenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		Material moduleMat,
		HashSet<ModuleLocation> prevLoaded = null
	)
	{

		var module = new Module(moduleSize, moduleMat);
		SetGround(module, moduleLocation);
		return new GenerationResponse
		{
			GeneratedAllModules = false,
			GeneratedModules = new Dictionary<ModuleLocation, Module>
			{
				{moduleLocation, module}
			},
			MaxBlockPos = new(Vector3I.One * (moduleSize - 1)),
			MinBlockPos = new(Vector3I.Zero),
		};
	}

	private void SetGround(Module module, ModuleLocation moduleLocation)
	{
		for (int x = 0; x < moduleSize; x++)
		{
			for (int z = 0; z < moduleSize; z++)
			{
				float xPos = (moduleLocation.Value.X * moduleSize + x + genOffset.X) * noiseScale;
				float zPos = (moduleLocation.Value.Z * moduleSize + z + genOffset.Z) * noiseScale;

				int noiseHeight = (int)(noise.GetNoise2D(xPos, zPos) * heightScale) + genOffset.Y;

				int maxY = Mathf.Min(noiseHeight - moduleLocation.Value.Y * moduleSize, moduleSize);

				for (int y = 0; y < maxY; y++)
				{
					ModuleGridPos inModulePos = new(new(x, y, z));
					module.SetBlock(inModulePos, 1);
				}
			}
		}
	}

	public override bool IsModuleNeeded(ModuleLocation moduleLocation)
	{
		return moduleLocation.Value.Y <= (int)(heightScale / moduleSize);
	}
}
