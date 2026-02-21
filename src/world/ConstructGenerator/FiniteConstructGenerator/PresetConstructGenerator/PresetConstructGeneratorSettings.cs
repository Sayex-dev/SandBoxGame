using System.Linq;
using Godot;

[GlobalClass]
public partial class PresetConstructGeneratorSettings : ConstructGeneratorSettings
{
    [Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
    [Export] public Vector3I Offset { get; set; }

    public override PresetConstructGenerator CreateConstructGenerator(int moduleSize, int seed)
    {
        return new(moduleSize, seed, Blocks.ToList(), Offset);
    }
}