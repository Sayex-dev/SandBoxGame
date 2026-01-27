using Godot;

[GlobalClass]
public partial class Biome : Resource
{
    [Export] public Godot.Collections.Array<NoiseLayer> NoiseLayers { get; set; }
    [Export] public BlockDefault block { get; set; }
    [Export] private FastNoiseLite noise = new FastNoiseLite();

    public virtual int GetBlockId(ConstructGridPos constructPos, int groundHeight, int seed)
    {
        return block.BlockId;
    }

    public virtual int GetGroundHeight(Vector2I inConstructPos, int seed)
    {
        float yLevel = 0;
        for (int i = 0; i < NoiseLayers.Count; i++)
        {
            yLevel += NoiseLayers[i].GetNoiseHeight2D((Vector2)inConstructPos, noise);
        }
        return (int)yLevel;
    }
}
