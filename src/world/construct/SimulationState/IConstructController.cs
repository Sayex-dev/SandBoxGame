public interface IConstructController
{
    void UpdateLoading(WorldGridPos loadPos);
    void Update(double delta);
    void SetBlock(Block block, ConstructGridPos pos);
    void RemoveBlock(ConstructGridPos pos);
    bool TryGetBlock(ConstructGridPos pos, out Block block);
}