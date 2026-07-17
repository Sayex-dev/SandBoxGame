using Godot;

[GlobalClass]
public partial class Structure : Resource
{
    [Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
}