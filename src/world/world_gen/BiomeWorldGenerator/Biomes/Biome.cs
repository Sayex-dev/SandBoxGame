using Godot;

[GlobalClass]
public partial class Biome : Resource
{
    [Export] public Godot.Collections.Array<NoiseLayer> NoiseLayers { get; set; }
    private FastNoiseLite noise = new FastNoiseLite();

    //    public int GetBlockId(Vector3I worldPos, int groundHeight, int seed)
    //    {
    //        noise.Seed = seed;
    //        
    //    }

    public int GetGroundHeight(Vector2I worldPos, int seed)
    {
        noise.Seed = seed;
        float yLevel = 0;
        for (int i = 0; i < NoiseLayers.Count; i++)
        {
            NoiseLayer noiseLayer = NoiseLayers[i];
            noise.NoiseType = noiseLayer.NoiseType;
            yLevel += (noise.GetNoise2Dv((Vector2)worldPos * noiseLayer.NoiseScale) * noiseLayer.NoiseHeight) + noiseLayer.NoiseHeightOffset;
        }
        return (int)yLevel;
    }
}
