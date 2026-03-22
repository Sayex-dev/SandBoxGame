using Godot;

[GlobalClass]
public partial class ModelBlockDefault : BlockDefault
{
    [Export] public BlockFaceResource DefaultFace = new BlockFaceResource();
    [Export] public Godot.Collections.Dictionary<Direction, BlockFaceResource> Faces = new Godot.Collections.Dictionary<Direction, BlockFaceResource>();
}