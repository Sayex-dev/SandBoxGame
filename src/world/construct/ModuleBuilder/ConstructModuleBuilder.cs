using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public class LoadAroundResponse
{
    public IEnumerable<Task<GenerateModulesResponse>> GenerationTaskHandles = new List<Task<GenerateModulesResponse>>();
    public List<ModuleLocation> ToUnload = new();
}

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
    public Dictionary<ModuleLocation, Mesh> Meshes = new();

    public void AddBlockResponse(ModuleBlockGenerationResponse blockGenResponse)
    {
        GeneratedAllModules = blockGenResponse.GeneratedAllModules;
        GeneratedModules = blockGenResponse.GeneratedModules;
    }
}

public class ConstructModuleBuilder : IDisposable
{
    private const int MaxConcurrentModuleLoads = 20;
    private readonly HashSet<ModuleLocation> _queued = new();
    SemaphoreSlim loadSemaphore = new SemaphoreSlim(MaxConcurrentModuleLoads);

    public LoadAroundResponse GenerateModulesAround(
        WorldGridPos worldPos,
        Vector3I renderDistance,
        ConstructTransform transform,
        ConstructModuleController modules,
        ModuleLoadContext context)
    {
        var center = worldPos.ToModuleLocation(transform, modules.ModuleSize);
        var diff = CalculateLoadSet(center, renderDistance, modules.Modules, context.Generator);

        var genTaskHandles = GenerateModuleGenerationTasks(diff.ToLoad, context);
        return new LoadAroundResponse()
        {
            GenerationTaskHandles = genTaskHandles,
            ToUnload = diff.ToUnload,
        };
    }

    public async Task<Mesh> GenerateModuleMesh(
        ModuleMeshGenerateContext context
    )
    {
        await loadSemaphore.WaitAsync();
        try
        {
            return await GenerateModuleMeshThreaded(context);
        }
        finally
        {
            loadSemaphore.Release();
        }
    }

    private IEnumerable<Task<GenerateModulesResponse>> GenerateModuleGenerationTasks(
        List<ModuleLocation> positions,
        ModuleLoadContext context
    )
    {
        var tasks = positions.Select(async pos =>
        {
            await loadSemaphore.WaitAsync();
            try
            {
                return await StartModuleGenerationThread(pos, context);
            }
            finally
            {
                loadSemaphore.Release();
            }
        }).ToList();

        return tasks;
    }

    private Task<Mesh> GenerateModuleMeshThreaded(ModuleMeshGenerateContext context)
    {
        return Task.Run(() =>
            {
                return ModuleMeshGenerator.BuildModuleMesh(context);
            });
    }

    private Task<GenerateModulesResponse> StartModuleGenerationThread(
    ModuleLocation location,
    ModuleLoadContext context)
    {
        return Task.Run(() =>
        {
            var response = new GenerateModulesResponse();
            TimeTracker.Start("Module Generation", TimeTracker.TrackingType.Average);
            var blockGenResponse = context.Generator.GenerateModules(location);
            TimeTracker.End("Module Generation");
            response.GeneratedAllModules = blockGenResponse.GeneratedAllModules;

            foreach (var kvp in blockGenResponse.GeneratedModules)
            {
                ModuleLocation moduleLoc = kvp.Key;
                Module module = kvp.Value;

                if (!module.HasBlocks) continue;

                response.GeneratedModules[moduleLoc] = module;
                var meshContext = new ModuleMeshGenerateContext(
                    module,
                    moduleLoc,
                    context.BlockStore,
                    context.ModuleMaterial
                );
                TimeTracker.Start("Build Module Mesh", TimeTracker.TrackingType.Average);
                var moduleMesh = ModuleMeshGenerator.BuildModuleMesh(meshContext);
                TimeTracker.End("Build Module Mesh");

                response.Meshes[moduleLoc] = moduleMesh;
            }

            return response;
        });
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

    public void Dispose() => loadSemaphore?.Dispose();
}