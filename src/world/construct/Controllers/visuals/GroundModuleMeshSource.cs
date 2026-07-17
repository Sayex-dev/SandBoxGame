using System;
using System.Collections.Generic;
using Godot;

public sealed class GroundModuleMeshSource : IModuleMeshSource
{
    public event Action<ModuleLocation, Mesh> MeshReady;
    public event Action<ModuleLocation> MeshRemoved;

    private readonly Dictionary<ModuleLocation, Mesh> _currentMeshes = new();

    public IEnumerable<KeyValuePair<ModuleLocation, Mesh>> CurrentMeshes => _currentMeshes;

    private readonly ConstructBlockController _modules;
    private readonly ConstructModuleBuilder _builder;

    public GroundModuleMeshSource(
        ConstructBlockController modules,
        ConstructModuleBuilder builder)
    {
        _modules = modules;
        _builder = builder;

        _modules.OnModuleAdded += OnModuleAdded;
        _modules.OnModuleRemoved += OnModuleRemoved;
    }

    private async void OnModuleAdded(ModuleLocation location, Module module)
    {
        Mesh mesh = await _builder.GenerateModuleMesh(new ModuleMeshGenerateContext(module, location));

        _currentMeshes[location] = mesh;
        MeshReady?.Invoke(location, mesh);
    }

    private void OnModuleRemoved(ModuleLocation location, Module module)
    {
        _currentMeshes.Remove(location);
        MeshRemoved?.Invoke(location);
    }

    public void Dispose()
    {
        _modules.OnModuleAdded -= OnModuleAdded;
        _modules.OnModuleRemoved -= OnModuleRemoved;
    }
}