using Godot;

[GlobalClass]
public partial class BlockStore : Resource
{
	[Export] public Godot.Collections.Array<BlockDefault> blockDefaults = [];

	public void SetBlockIds()
	{
		for (int i = 0; i < blockDefaults.Count; i++)
		{
			blockDefaults[i].BlockId = i;
		}
	}
}
