using Godot;


[GlobalClass]
public abstract partial class ConstructGeneratorSettings : Resource
{
    public abstract ConstructGenerator CreateConstructGenerator(int moduleSize, int seed);
}
