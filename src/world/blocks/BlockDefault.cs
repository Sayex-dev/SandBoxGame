using Godot;
using System;

[GlobalClass]
public partial class BlockDefault : Resource
{
	[Export] public string Name { get; set; }
	[Export] public int Health { get; set; }
	[Export] public float Weight { get; set; }

	[Export] public Godot.Collections.Array<PassiveAbility> PassiveAbilities { get; set; }
	[Export] public Godot.Collections.Array<ActiveAbility> ActiveAbilities { get; set; }

	[Export] public Vector2I TextureAtlasFaceUp { get; set; }
	[Export] public Vector2I TextureAtlasFaceDown { get; set; }
	[Export] public Vector2I TextureAtlasFaceLeft { get; set; }
	[Export] public Vector2I TextureAtlasFaceRight { get; set; }
	[Export] public Vector2I TextureAtlasFaceForward { get; set; }
	[Export] public Vector2I TextureAtlasFaceBackward { get; set; }

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
