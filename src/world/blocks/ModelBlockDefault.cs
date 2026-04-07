using Godot;

[GlobalClass]
public partial class ModelBlockDefault : BlockDefault
{
    [Export] public BlockFaceResource LodBlockFace;
    [Export] public Mesh Mesh;
}