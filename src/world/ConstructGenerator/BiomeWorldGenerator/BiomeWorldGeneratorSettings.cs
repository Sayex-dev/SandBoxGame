using Godot;

[GlobalClass]
public partial class BiomeWorldGeneratorSettings : ConstructGeneratorSettings
{
    [Export] public Godot.Collections.Array<Biome> Biomes { get; set; }

    public override ConstructGenerator CreateConstructGenerator(int moduleSize, int seed)
    {
        return new BiomeWorldGenerator(moduleSize, seed, Biomes);
    }
}