using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class ConstructWorld : Node3D, IWorldQuery
{
	private int seed;
	private Material moduleMaterial;
	private List<Tuple<SimulationMode, float>> simulationModeDistances;

	private ExpandingOctTree<Construct> constructTree;
	private HashSet<Construct> constructs = [];
	private Vector3 lastCameraPos = Vector3I.Zero;

	public void Initialize(
		Material moduleMaterial
	)
	{
		this.moduleMaterial = moduleMaterial;
	}

	public override void _Ready()
	{
		constructTree = new ExpandingOctTree<Construct>(32, Vector3I.Zero);
		GatherChildConstructs();
	}

	public Construct HasBlock(WorldGridPos worldPos)
	{
		List<Construct> nearConstructs = constructTree.QueryAt(worldPos);
		foreach (Construct construct in nearConstructs)
		{
			if (construct.TryGetBlock(worldPos, out _))
			{
				return construct;
			}
		}
		return null;
	}

	/// <summary>
	/// Adds a finite construct and loads all its modules once.
	/// </summary>
	public void AddConstruct(Construct construct)
	{
		if (construct.Core.Data.PhysicsData.IsStatic)
			constructTree.InsertGlobal(construct);
		else
			constructTree.Insert(construct);
		constructs.Add(construct);
	}

	public bool HasBlockAt(WorldGridPos worldPos)
	{
		return HasBlock(worldPos) != null;
	}

	public List<Construct> GetConstructsInArea(WorldGridPos min, WorldGridPos max)
	{
		return constructTree.QueryBox(min.Value, max.Value);
	}

	public void CameraMoved(Vector3 newPos)
	{
		lastCameraPos = newPos;
		foreach (var construct in constructs)
		{
			construct.UpdateLoading((Vector3I)newPos);
		}
	}

	private void GatherChildConstructs()
	{
		List<ConstructNode> childNodes = this.FindChildrenOfType<ConstructNode>();
		foreach (var childNode in childNodes)
		{
			AddConstruct(childNode.CreateConstruct(this, moduleMaterial, this, (Vector3I)lastCameraPos));
		}
	}
}
