using Godot;

[GlobalClass]
public partial class StructurePlacement : Resource
{
    [Export] public Structure structure;

    private FastNoiseLite noise = new FastNoiseLite();

    public StructurePlacement()
    {
        noise.Get
    }

    public Vector3I GetClosest(int seed)
    {

    }
}

[GlobalClass]
public partial class Biome : Resource
{
    public abstract Godot.Collections.Dictionary<>
}
