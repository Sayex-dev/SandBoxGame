using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ConstructModelBlockController : IDisposable, IConstructBlockVisuals
{
    private record MeshData(MultiMeshInstance3D Instance, HashSet<ConstructGridPos> Positions);

    private Dictionary<ModelBlockDefault, MeshData> modelMeshInstances = new();
    private Dictionary<ConstructGridPos, ModelBlockDefault> positionToModel = new();

    private Node3D construct;
    private ConstructData data;

    public ConstructModelBlockController(Node3D construct, ConstructData data)
    {
        this.construct = construct;
        this.data = data;

        data.Modules.OnModuleChanged += OnModulesChanged;
        data.Modules.OnModuleAdded += OnModulesAdded;
        data.Modules.OnModuleRemoved += OnModulesRemoved;
    }

    public void SetBlock(ModuleLocation location, BlockChange[] changes)
    {
        HashSet<ModuleLocation> changedModules = [];
        for (int i = 0; i < changes.Length; i++)
        {
            var change = changes[i];
            var pos = changes[i].Position.ToConstruct(location);

            switch (change.Action)
            {
                case BlockChangeAction.PLACE:
                    RemoveBlock(pos);
                    AddBlock(pos, change.Block);
                    changedModules.Add(pos.ToModuleLocation());
                    break;
                case BlockChangeAction.REMOVE:
                    RemoveBlock(pos);
                    changedModules.Add(pos.ToModuleLocation());
                    break;
            }
        }

        UpdateAllMultiMeshes();
    }

    private void OnModulesChanged(ModuleLocation location, BlockChange[] changes) =>
        SetBlock(location, changes);

    private void OnModulesAdded(ModuleLocation location, Module module)
    {
        foreach (ModuleGridPos pos in module.ModelBlockPositions)
        {
            Block block = module.GetBlock(pos);
            ConstructGridPos constructPos = pos.ToConstruct(location);
            AddBlock(constructPos, block);
        }
    }

    private void OnModulesRemoved(ModuleLocation location, Module module)
    {
        var toRemove = positionToModel.Keys
            .Where(p => p.ToModuleLocation() == location)
            .ToList();
        foreach (var pos in toRemove)
            RemoveBlock(pos);
    }

    public void AddBlock(ConstructGridPos pos, Block block)
    {
        var blockDefault = BlockStore.Instance.GetBlockDefault(block);
        AddBlock(pos, blockDefault);
    }

    public void AddBlock(ConstructGridPos pos, BlockDefault blockDefault)
    {
        if (blockDefault is ModelBlockDefault modelBlockDefault)
        {
            if (!modelMeshInstances.ContainsKey(modelBlockDefault))
            {
                var meshInstance = new MultiMeshInstance3D();
                construct.AddChild(meshInstance);
                var multiMeshInstance = new MultiMesh
                {
                    Mesh = modelBlockDefault.Mesh,
                    TransformFormat = MultiMesh.TransformFormatEnum.Transform3D
                };
                meshInstance.Multimesh = multiMeshInstance;
                modelMeshInstances[modelBlockDefault] = new MeshData(meshInstance, new());
            }

            modelMeshInstances[modelBlockDefault].Positions.Add(pos);
            positionToModel[pos] = modelBlockDefault;

            UpdateMultiMesh(modelBlockDefault);
        }
    }

    public void AddBlocks(ConstructGridPos[] positions, Block[] blocks)
    {
        foreach (var (pos, block) in positions.Zip(blocks))
        {
            AddBlock(pos, block);
        }
    }

    public void RemoveBlock(ConstructGridPos pos)
    {
        if (positionToModel.TryGetValue(pos, out var modelBlockDefault))
        {
            modelMeshInstances[modelBlockDefault].Positions.Remove(pos);
            positionToModel.Remove(pos);

            if (modelMeshInstances[modelBlockDefault].Positions.Count == 0)
            {
                modelMeshInstances[modelBlockDefault].Instance.QueueFree();
                modelMeshInstances.Remove(modelBlockDefault);
            }
            else
            {
                UpdateMultiMesh(modelBlockDefault);
            }
        }
    }

    private void UpdateMultiMesh(ModelBlockDefault modelBlockDefault)
    {
        if (!modelMeshInstances.TryGetValue(modelBlockDefault, out var meshData))
            return;

        var positions = meshData.Positions.ToArray();
        meshData.Instance.Multimesh.InstanceCount = positions.Length;

        var basis = Basis.Identity.Scaled(modelBlockDefault.Scale);
        for (int i = 0; i < positions.Length; i++)
        {
            var origin = (Vector3)(Vector3I)positions[i] + modelBlockDefault.Offset;
            var transform = new Transform3D(basis, origin);
            meshData.Instance.Multimesh.SetInstanceTransform(i, transform);
        }
    }

    private void UpdateAllMultiMeshes()
    {
        foreach (var kvp in modelMeshInstances)
        {
            UpdateMultiMesh(kvp.Key);
        }
    }

    public void Dispose()
    {
        data.Modules.OnModuleChanged -= OnModulesChanged;
        data.Modules.OnModuleAdded -= OnModulesAdded;
        data.Modules.OnModuleRemoved -= OnModulesRemoved;

        foreach (var meshData in modelMeshInstances.Values)
        {
            meshData.Instance?.QueueFree();
        }

        modelMeshInstances.Clear();
        positionToModel.Clear();
        construct = null;
    }
}