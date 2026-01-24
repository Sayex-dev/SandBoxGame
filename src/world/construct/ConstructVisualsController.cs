using System.Collections.Generic;
using Godot;

public partial class ConstructVisualsController
{
    private Node parent;
    private float moduleSize;
    private Dictionary<ModuleLocation, MeshInstance3D> activeModules = new();
    private LinkedList<MeshInstance3D> modulePool = new();

    public ConstructVisualsController(float moduleSize, Node parent, int initialPoolSize = 0)
    {
        this.parent = parent;
        this.moduleSize = moduleSize;
        for (int i = 0; i < initialPoolSize; i++)
        {
            ExtendModulePool();
        }
    }

    public void AddModule(ModuleLocation loc, Mesh moduleMesh)
    {
        RemoveModule(loc);
        if (modulePool.First == null) ExtendModulePool();
        var module = modulePool.First.Value;
        modulePool.RemoveFirst();
        module.Mesh = moduleMesh;
        activeModules[loc] = module;
        module.Position = (Vector3)loc.Value * moduleSize;
    }

    public void RemoveModule(ModuleLocation loc)
    {
        activeModules.Remove(loc, out MeshInstance3D module);
        if (module == null) return;
        module.Mesh = null;
        modulePool.AddFirst(module);
    }

    private void ExtendModulePool()
    {
        MeshInstance3D newModule = new MeshInstance3D();
        modulePool.AddFirst(newModule);
        parent.AddChild(newModule);
    }
}