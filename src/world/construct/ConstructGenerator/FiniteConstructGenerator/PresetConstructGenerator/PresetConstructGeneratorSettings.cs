using System.Linq;
using Godot;

[GlobalClass]
public partial class PresetConstructGeneratorSettings : ConstructGeneratorSettings
{
    [Export] public Godot.Collections.Dictionary<Vector3I, GodotBlock> Blocks;
    [Export] public Vector3I Offset;

    public override PresetConstructGenerator CreateConstructGenerator(int moduleSize, int seed)
    {
        var blockDict = Blocks.ToDictionary(kvp => kvp.Key, kvp => new Block(kvp.Value.BlockId, kvp.Value.FaceDir));
        return new(moduleSize, seed, blockDict, Offset);
    }
}