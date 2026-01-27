using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public class GenerationReference
{
    public int ModuleSize;
    public BlockStore BlockStore;
    public Material ModuleMaterial;
    public ConstructGenerator Generator;
    public Dictionary<ModuleLocation, Module> LoadedModules;
}

public class GenerateModulesResponse
{
    public bool GeneratedAllModules = false;
    public Dictionary<ModuleLocation, Module> GeneratedModules = [];
    public ExposedModuleSurfaceCache Cache;
    public Dictionary<ModuleLocation, Mesh> Meshes;

    public void AddBlockResponse(ModuleBlockGenerationResponse blockGenResponse)
    {
        GeneratedAllModules = blockGenResponse.GeneratedAllModules;
        GeneratedModules = blockGenResponse.GeneratedModules;
    }
}

public class ConstructModuleBuilder
{
    private const int MaxConcurrentModuleLoads = 5;
    private readonly HashSet<ModuleLocation> _queued = new();

    public async Task<IEnumerable<Task<GenerateModulesResponse>>> GenerateModulesAround(
        WorldGridPos worldPos,
        Vector3I renderDistance,
        ConstructTransform transform,
        ConstructModuleController modules,
        ModuleLoadContext context)
    {
        var center = worldPos.ToModuleLocation(transform, modules.ModuleSize);
        var diff = CalculateLoadSet(center, renderDistance, modules.Modules, context.Generator);

        UnloadModules(diff.ToUnload, modules);
        return GenerateModuleTasks(diff.ToLoad, context);
    }

    public async Task<Mesh> GenerateModuleMesh(
        ModuleMeshGenerateContext context
    )
    {
        using var semaphore = new SemaphoreSlim(MaxConcurrentModuleLoads);

        await semaphore.WaitAsync();
        try
        {
            return await GenerateModuleMeshThreaded(context);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private IEnumerable<Task<GenerateModulesResponse>> GenerateModuleTasks(
        List<ModuleLocation> positions,
        ModuleLoadContext context
    )
    {
        using var semaphore = new SemaphoreSlim(MaxConcurrentModuleLoads);

        var tasks = positions.Select(async pos =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await GenerateModulesThreaded(pos, context);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return tasks;
    }

    private Task<Mesh> GenerateModuleMeshThreaded(ModuleMeshGenerateContext context)
    {
        return Task.Run(() =>
        {
            return ModuleMeshGenerator.BuildModuleMesh(context);
        });
    }

    private Task<GenerateModulesResponse> GenerateModulesThreaded(
    ModuleLocation location,
    ModuleLoadContext context)
    {
        return Task.Run(() =>
        {
            var response = new GenerateModulesResponse();
            var blockGenResponse = context.Generator.GenerateModules(location);

            foreach (var kvp in response.GeneratedModules)
            {
                ModuleLocation moduleLoc = kvp.Key;
                Module module = kvp.Value;

                if (!module.HasBlocks) continue;
                var meshContext = new ModuleMeshGenerateContext(
                    module,
                    moduleLoc,
                    context.BlockStore,
                    context.ModuleMaterial
                );
                var moduleMesh = ModuleMeshGenerator.BuildModuleMesh(meshContext);

                response.Meshes[moduleLoc] = moduleMesh;
            }

            return response;
        });
    }


    private void UnloadModules(
        IEnumerable<ModuleLocation> positions,
        ConstructModuleController modules
    )
    {
        foreach (var pos in positions)
        {
            modules.Remove(pos, out _);
        }
    }

    private ModuleLoadSet CalculateLoadSet(
        ModuleLocation center,
        Vector3I renderDistance,
        Dictionary<ModuleLocation, Module> loaded,
        ConstructGenerator generator
    )
    {
        var result = new ModuleLoadSet();
        var desired = new HashSet<ModuleLocation>();

        for (int x = -renderDistance.X; x < renderDistance.X; x++)
            for (int y = -renderDistance.Y; y < renderDistance.Y; y++)
                for (int z = -renderDistance.Z; z < renderDistance.Z; z++)
                {
                    var pos = new ModuleLocation(center.Value + new Vector3I(x, y, z));
                    if (generator.IsModuleNeeded(pos))
                        desired.Add(pos);
                }

        foreach (var pos in desired)
            if (!loaded.ContainsKey(pos) && _queued.Add(pos))
                result.ToLoad.Add(pos);

        foreach (var pos in loaded.Keys.ToList())
            if (!desired.Contains(pos))
                result.ToUnload.Add(pos);

        return result;
    }
}