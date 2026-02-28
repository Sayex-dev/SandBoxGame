using Godot;

[GlobalClass]
public partial class GodotBlock : Resource
{
    [Export] public int BlockId = 1;
    [Export] public Direction FaceDir = Direction.FORWARD;
}