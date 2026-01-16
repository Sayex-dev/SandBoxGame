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

    public virtual int GetBlockId(ConstructGridPos constructPos, int groundHeight)
    {
        return block.BlockId;
    }

    public virtual int GetGroundHeight(Vector2I inConstructPos)
    {
        float yLevel = 0;
        for (int i = 0; i < NoiseLayers.Count; i++)
        {
            yLevel += NoiseLayers[i].GetNoiseHeight2D((Vector2)inConstructPos, noise);
        }
        return (int)yLevel;
    }
}
