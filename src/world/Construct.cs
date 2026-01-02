using Godot;
using System.Collections.Generic;

public partial class Construct : Node
{
	private Dictionary<Vector3I, Module> modules = new();
	private List<Vector3I> queuedModulesPositions = new();
}
