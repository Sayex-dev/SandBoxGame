using Godot;

[GlobalClass]
public partial class Biome : Resource
{
    [Export] public Godot.Collections.Array<NoiseLayer> NoiseLayers { get; set; }
    [Export] public BlockDefault block { get; set; }
    private FastNoiseLite noise = new FastNoiseLite();

    public void SetSeed(int seed)
    {
        noise.Seed = seed;
    }

    public virtual int GetBlockId(Vector3I worldPos, int groundHeight)
    {
        return block.BlockId;
    }

    public virtual int GetGroundHeight(Vector2I worldPos)
    {
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
