using System.Collections.Generic;
using Godot;

public partial class ConstructVisualsController : Node3D
{
    private float moduleSize;
    private Dictionary<ModuleLocation, MeshInstance3D> activeModules = new();
    private LinkedList<MeshInstance3D> modulePool = new();
    private ConstructModulesData modules;

    public ConstructVisualsController(ConstructModulesData modules, int initialPoolSize = 0)
    {
        this.modules = modules;
        modules.OnModuleAdded += OnModuleAdded;
        modules.OnModuleRemoved += OnModuleRemoved;

        moduleSize = GameSettings.Instance.ModuleSize;
        for (int i = 0; i < initialPoolSize; i++)
        {
            ExtendModulePool();
        }
    }

    public void AddModule(ModuleLocation loc, Mesh moduleMesh)
    {
        RemoveModule(loc);
        if (modulePool.First == null)
            ExtendModulePool();
        var module = modulePool.First.Value;
        modulePool.RemoveFirst();
        module.Mesh = moduleMesh;
        activeModules[loc] = module;
        module.Position = (Vector3)loc.Value * moduleSize;
    }

    public void RemoveModule(ModuleLocation loc)
    {
        activeModules.Remove(loc, out MeshInstance3D module);
        if (module == null)
            return;
        module.Mesh = null;
        modulePool.AddFirst(module);
    }

    public void CreateOrUpdateModule(ModuleLocation moduleLoc, Mesh mesh)
    {
        MeshInstance3D module;
        if (!activeModules.TryGetValue(moduleLoc, out module))
        {
            AddModule(moduleLoc, mesh);
        }
        else
        {
            module.Mesh = mesh;
        }
    }

    public void OnTreeExiting()
    {
        modules.OnModuleChanged -= OnModuleChanged;
    }

    private void ExtendModulePool()
    {
        MeshInstance3D newModule = new MeshInstance3D();
        modulePool.AddFirst(newModule);
        AddChild(newModule);
    }

    private void OnModuleAdded(ModuleLocation location, Module module)
    {

    }

    private void OnModuleRemoved(ModuleLocation location, Module module)
    {

    }
}