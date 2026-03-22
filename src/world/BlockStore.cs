using Godot;

[GlobalClass]
public partial class BlockStore : Node
{
	public static BlockStore Instance { get; private set; }

	[Export] private Godot.Collections.Array<BlockDefault> blockDefaults = [];

	public override void _Ready()
	{
		Instance = Instance == null ? this : Instance;
		SetBlockIds();
	}

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
