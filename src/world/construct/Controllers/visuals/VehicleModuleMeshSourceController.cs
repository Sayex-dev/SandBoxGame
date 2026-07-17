using System;
using System.Collections.Generic;
using Godot;

public sealed class VehicleModuleMeshSource : IModuleMeshSource
{
    public event Action<ModuleLocation, Mesh> MeshReady;
    public event Action<ModuleLocation> MeshRemoved;

    private readonly Dictionary<ModuleLocation, Mesh> _meshes = new();

    public IEnumerable<KeyValuePair<ModuleLocation, Mesh>> CurrentMeshes => _meshes;

    public VehicleModuleMeshSource(Dictionary<ModuleLocation, Mesh> meshes)
    {
        _meshes = meshes;
    }

    public void UpdateModule(ModuleLocation location, Mesh mesh)
    {
        _meshes[location] = mesh;
        MeshReady?.Invoke(location, mesh);
    }

    public void RemoveModule(ModuleLocation location)
    {
        if (_meshes.Remove(location))
        {
            MeshRemoved?.Invoke(location);
        }
    }
}