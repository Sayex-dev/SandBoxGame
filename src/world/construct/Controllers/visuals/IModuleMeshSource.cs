using System;
using System.Collections.Generic;
using Godot;

public interface IModuleMeshSource
{
    event Action<ModuleLocation, Mesh> MeshReady;
    event Action<ModuleLocation> MeshRemoved;

    IEnumerable<KeyValuePair<ModuleLocation, Mesh>> CurrentMeshes { get; }
}