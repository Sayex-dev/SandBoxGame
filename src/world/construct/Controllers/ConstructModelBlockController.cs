using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ConstructModelBlockController : IDisposable
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
        data.Modules.OnSetBlock += OnSetBlock;
    }

    private void OnSetBlock(BlockChange[] changes)
    {
        HashSet<ModuleLocation> changedModules = [];
        for (int i = 0; i < changes.Length; i++)
        {
            var change = changes[i];
            var pos = changes[i].Position;

            switch (change.Action)
            {
                case BlockChangeAction.REPLACE:
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

        for (int i = 0; i < positions.Length; i++)
        {
            var transform = new Transform3D(Basis.Identity, (Vector3I)positions[i]);
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
        if (data != null)
        {
            data.Modules.OnSetBlock -= OnSetBlock;
        }

        foreach (var meshData in modelMeshInstances.Values)
        {
            meshData.Instance?.QueueFree();
        }

        modelMeshInstances.Clear();
        positionToModel.Clear();
        construct = null;
        data = null;
    }
}