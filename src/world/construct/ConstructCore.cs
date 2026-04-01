using Godot;

public class ConstructCore
{
    public ConstructData Data { get; }
    public ConstructBlockService Blocks { get; }
    public Node3D ConstructNode { get; private set; }

    public ConstructCore(ConstructData data, ConstructBlockService blocks, Node3D constructNode)
    {
        Data = data;
        Blocks = blocks;
        ConstructNode = constructNode;
    }
}