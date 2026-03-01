using Godot;

[GlobalClass]
public partial class BlockFace : Resource
{
    [Export] public Vector2I TextureAtlasPos;
    [Export] public Orientation FaceOrientation = Orientation.NORTH;
}