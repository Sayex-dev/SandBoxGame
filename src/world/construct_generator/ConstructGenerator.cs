using System.Collections.Generic;
using Godot;

public class GenerationResponse
{
	public bool generatedAllModules = false;
	public Dictionary<Vector3I, Module> generatedModules = [];

}

[GlobalClass]
public abstract partial class ConstructGenerator : Resource
{
	public abstract GenerationResponse GenerateModules(Vector3I relativeWorldPos, Material moduleMaterial, int moduleSize);
	public abstract void SetSeed(int seed);
}
