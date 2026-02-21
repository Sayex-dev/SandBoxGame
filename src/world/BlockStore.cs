using System.Dynamic;
using Godot;

[GlobalClass]
public partial class BlockStore : Resource
{
	[Export] private Godot.Collections.Array<BlockDefault> blockDefaults = [];

	public void SetBlockIds()
	{
		for (int i = 0; i < blockDefaults.Count; i++)
		{
			blockDefaults[i].Id = i + 1;
		}
	}

	public BlockDefault GetBlockDefault(int id)
	{
		return blockDefaults[id - 1];
	}

	public BlockDefault GetBlockDefault(Block block)
	{
		return GetBlockDefault(block.Id);
	}
}
