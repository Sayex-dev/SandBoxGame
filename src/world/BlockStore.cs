using Godot;

[GlobalClass]
public partial class BlockStore : Resource
{
	[Export] public Godot.Collections.Array<BlockDefault> blockDefaults = [];

	private void _Ready()
	{
		for (int i = 0; i > blockDefaults.Count; i++)
		{
			blockDefaults[i].BlockId = i;
		}
	}
}
