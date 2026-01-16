using Godot;

[GlobalClass]
public partial class SimpleNoiseConstructGeneratorSettings : ConstructGeneratorSettings
{
    [Export] public Vector3I GenOffset { get; set; }
    [Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Simplex;
    [Export] public float NoiseScale { get; set; } = 1;
    [Export] public float HeightScale { get; set; } = 10;

    public override SimpleNoiseConstructGenerator CreateConstructGenerator(int moduleSize, int seed)
    {
        return new(moduleSize, seed, NoiseScale, HeightScale, GenOffset, NoiseType);
    }
}