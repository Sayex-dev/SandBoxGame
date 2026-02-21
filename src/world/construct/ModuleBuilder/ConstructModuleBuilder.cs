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
    private const int MaxConcurrentModuleLoads = 7;
    private readonly HashSet<ModuleLocation> _queued = new();
    SemaphoreSlim loadSemaphore = new SemaphoreSlim(MaxConcurrentModuleLoads);

    public LoadAroundResponse GenerateModulesAround(
        WorldGridPos worldPos,
        int loadDistance,
        ConstructTransform transform,
        ConstructModules modules,
        ModuleLoadContext context)
    {
        var center = worldPos.ToModuleLocation(transform, modules.ModuleSize);
        var diff = CalculateLoadSet(center, loadDistance, modules.Modules, context.Generator);

        var genTaskHandles = GenerateModuleGenerationTasks(diff.ToLoad, context);
        return new LoadAroundResponse()
        {
            GenerationTaskHandles = genTaskHandles,
            ToUnload = diff.ToUnload,
        };
    }

    /// <summary>
    /// Generates all modules that the generator reports as needed.
    /// Unlike GenerateModulesAround, this does not use distance-based loading/unloading.
    /// It queries the generator for every required module and generates them all.
    /// Used for finite constructs that need to be fully loaded once.
    /// </summary>
    public IEnumerable<Task<GenerateModulesResponse>> GenerateAllModules(
        ModuleLoadContext context)
    {
        var allNeeded = context.Generator.GetAllRequiredModules();
        var toLoad = new List<ModuleLocation>();

        foreach (var pos in allNeeded)
        {
            if (_queued.Add(pos))
                toLoad.Add(pos);
        }

        return GenerateModuleGenerationTasks(toLoad, context);
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

                if (!module.HasBlocks)
                    continue;

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
        int renderDistance,
        Dictionary<ModuleLocation, Module> loaded,
        ConstructGenerator generator
    )
    {
        var sphereArea = (int)Math.Ceiling(4.0 / 3.0 * Math.PI * Math.Pow(renderDistance, 3));
        var result = new ModuleLoadSet();
        (ModuleLocation, float)[] desired = new (ModuleLocation, float)[sphereArea];
        HashSet<ModuleLocation> desiredLookup = new();

        int i = 0;
        for (int x = -renderDistance; x < renderDistance; x++)
            for (int z = -renderDistance; z < renderDistance; z++)
                for (int y = -renderDistance; y < renderDistance; y++)
                {
                    var distVec = new Vector3I(x, y, z);
                    var pos = new ModuleLocation(center.Value + distVec);
                    var dist = distVec.Length();
                    if (generator.IsModuleNeeded(pos) && dist <= renderDistance)
                    {
                        desired[i] = (pos, dist);
                        desiredLookup.Add(pos);
                        i++;
                    }
                }

        // Add case
        List<(ModuleLocation, float)> resultList = new();
        foreach (var (pos, dist) in desired)
            if (!loaded.ContainsKey(pos) && _queued.Add(pos))
                resultList.Add((pos, dist));

        // Sort and assign
        resultList.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        foreach (var (pos, dist) in resultList)
            result.ToLoad.Add(pos);

        // Remove case
        foreach (var pos in loaded.Keys.ToList())
            if (!desiredLookup.Contains(pos))
            {
                _queued.Remove(pos);
                result.ToUnload.Add(pos);
            }
        return result;
    }

    public void Dispose() => loadSemaphore?.Dispose();
}