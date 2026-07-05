using Godot;

[GlobalClass]
public partial class ModelBlockDefault : BlockDefault
{
    [Export] public BlockFaceResource LodBlockFace;
    [Export] public Mesh Mesh;
    [Export] public Vector3 Offset = Vector3.Zero;
    [Export] public Vector3 Scale = Vector3.One;
}