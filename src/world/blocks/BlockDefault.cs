using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[GlobalClass]
public partial class BlockDefault : Resource
{
	[Export] public string Name { get; set; }
	[Export] public int Health { get; set; }

	[Export] public Godot.Collections.Array<PassiveAbility> PassiveAbilities { get; set; }
	[Export] public Godot.Collections.Array<ActiveAbility> ActiveAbilities { get; set; }

	[Export] public Vector2I TextureAtlasFaceUp { get; set; }
	[Export] public Vector2I TextureAtlasFaceDown { get; set; }
	[Export] public Vector2I TextureAtlasFaceLeft { get; set; }
	[Export] public Vector2I TextureAtlasFaceRight { get; set; }
	[Export] public Vector2I TextureAtlasFaceForward { get; set; }
	[Export] public Vector2I TextureAtlasFaceBackward { get; set; }

	private int blockId = -1;

	public int BlockId
	{
		get { return blockId; }
		set
		{
			if (blockId != -1)
			{
				throw new ArgumentException("Block id cannot be changed once set.");
			}
			blockId = value;
		}
	}

}
