using Godot;

public interface IStateController
{
    void UpdateLoading(WorldGridPos loadPos);
    void Update(double delta);
    Vector3 GetPosition();
    Vector3 GetRotation();
    void SetBlock(Block block, ConstructGridPos pos);
    void RemoveBlock(ConstructGridPos pos);
    bool TryGetBlock(ConstructGridPos pos, out Block block);
}