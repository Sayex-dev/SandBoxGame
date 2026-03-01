using Godot;
using System;

[GlobalClass]
public partial class BlockDefault : Resource
{
	[Export] public string Name;
	[Export] public int Health;
	[Export] public float Weight;

	[Export] public Godot.Collections.Array<PassiveAbility> PassiveAbilities;
	[Export] public Godot.Collections.Array<ActiveAbility> ActiveAbilities;

	[Export] public BlockFace DefaultFace = new BlockFace();
	[Export] public Godot.Collections.Dictionary<Direction, BlockFace> Faces = new Godot.Collections.Dictionary<Direction, BlockFace>();

	private int blockId = 0;

	public int Id
	{
		get { return blockId; }
		set
		{
			if (blockId != 0)
			{
				throw new ArgumentException("Block id cannot be changed once set.");
			}
			blockId = value;
		}
	}

}
