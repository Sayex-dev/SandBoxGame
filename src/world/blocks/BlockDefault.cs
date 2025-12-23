using Godot;
using System.Collections.Generic;

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
}
