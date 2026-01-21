using Godot;
using System.Collections.Generic;

public partial class ChildObjectPool : Node3D
{
    private Dictionary<ModuleLocation, Node3D> activeModules = new();
    private LinkedList<Node3D> modulePool = new();

    public void Add() // Make generic Dictionary Key and rename to ChildDictionaryPool
}