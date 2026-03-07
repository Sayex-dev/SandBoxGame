using Godot;

[GlobalClass]
public partial class BlockFaceResource : Resource
{
    [Export] public Vector2I TextureAtlasPos;
    [Export] public Orientation FaceOrientation = Orientation.NORTH;
}