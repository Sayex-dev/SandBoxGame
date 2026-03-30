using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[GlobalClass]
public partial class ConstructWorld : Node3D, IWorldQuery
{
	private int seed;
	private int moduleSize;
	private Material moduleMaterial;
	private List<Tuple<SimulationMode, float>> simulationModeDistances;

	private ExpandingOctTree<Construct> constructTree;
	private HashSet<Construct> constructs = [];
	private Vector3 lastCameraPos = Vector3I.Zero;

	public void Initialize(
		int seed,
		int moduleSize,
		Material moduleMaterial,
		List<Tuple<SimulationMode, float>> simulationModeDistances
	)
	{
		this.seed = seed;
		this.moduleSize = moduleSize;
		this.moduleMaterial = moduleMaterial;
		this.simulationModeDistances = simulationModeDistances;
	}

	public override void _Ready()
	{
		constructTree = new ExpandingOctTree<Construct>(32, Vector3I.Zero);
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
		SetConstructSimulationState(construct, lastCameraPos);
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
			SetConstructSimulationState(construct, newPos);
		}
	}

	private void SetConstructSimulationState(Construct construct, Vector3 newPos)
	{
		WorldGridPos constPos = construct.Core.Data.GridTransform.WorldPos;
		float dist = (newPos - (Vector3I)constPos).Length();
		construct.ChangeSimulationState(GetSimulationMode(dist));
	}

	private SimulationMode GetSimulationMode(float dist)
	{
		SimulationMode resultMode = simulationModeDistances[0].Item1;
		foreach ((var mode, var maxDist) in simulationModeDistances)
		{
			if (dist > maxDist)
				return resultMode;
			else
				resultMode = mode;
		}
		return resultMode;
	}
}
