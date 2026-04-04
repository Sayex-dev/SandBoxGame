using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public partial class ConstructModelBlockController
{
    private record MeshData(MultiMeshInstance3D Instance, HashSet<ConstructGridPos> Positions);

    private Dictionary<ModelBlockDefault, MeshData> modelMeshInstances = new();
    private Dictionary<ConstructGridPos, ModelBlockDefault> positionToModel = new();

    private Node3D construct;

    public ConstructModelBlockController(Node3D construct)
    {
        this.construct = construct;
    }

    public void AddBlock(ConstructGridPos pos, Block block)
    {
        var blockDefault = BlockStore.Instance.GetBlockDefault(block);
        if (blockDefault is ModelBlockDefault modelBlockDefault)
        {
            if (!modelMeshInstances.ContainsKey(modelBlockDefault))
            {
                var meshInstance = new MultiMeshInstance3D();
                construct.AddChild(meshInstance);
                var multiMeshInstance = new MultiMesh
                {
                    Mesh = modelBlockDefault.BlockMesh
                };
                meshInstance.Multimesh = multiMeshInstance;
                modelMeshInstances[modelBlockDefault] = new MeshData(meshInstance, new());
            }

            modelMeshInstances[modelBlockDefault].Positions.Add(pos);
            positionToModel[pos] = modelBlockDefault;
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
                // Clean up empty mesh
                modelMeshInstances[modelBlockDefault].Instance.QueueFree();
                modelMeshInstances.Remove(modelBlockDefault);
            }
        }
    }
}