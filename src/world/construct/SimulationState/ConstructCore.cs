public class ConstructCore
{
    public ConstructData Data { get; }
    public ConstructBlockService Blocks { get; }

    public ConstructCore(ConstructData data, ConstructBlockService blocks)
    {
        Data = data;
        Blocks = blocks;
    }
}