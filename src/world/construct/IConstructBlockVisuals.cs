/// <summary>
/// Shared contract for block visual renderers.
/// Implemented by both ConstructVisualsController (voxel greedy mesh)
/// and ConstructModelBlockController (model MultiMesh).
/// </summary>
public interface IConstructBlockVisuals
{
    void AddBlock(ConstructGridPos pos, Block block);
    void AddBlocks(ConstructGridPos[] positions, Block[] blocks);
    void RemoveBlock(ConstructGridPos pos);
}
