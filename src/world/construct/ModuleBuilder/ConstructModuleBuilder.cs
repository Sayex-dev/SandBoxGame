using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public class GenerationReference
{
    public int moduleSize;
    public BlockStore blockStore;
    public Material moduleMaterial;
    public ExposedSurfaceCache exposedSurfaceCache;
    public ConstructGenerator generator;
    public Dictionary<ModuleLocation, Module> loadedModules;
}

public partial class ConstructModuleBuilder
{
    private const int MaxConcurrentModuleLoads = 5;
    private readonly HashSet<ModuleLocation> _queued = new();

    public async Task<IEnumerable<Task<ModuleGenerationResponse>>> LoadAroundPosition(
        WorldGridPos worldPos,
        Vector3I renderDistance,
        ConstructGridTransform transform,
        ConstructModuleController modules,
        ModuleLoadContext context)
    {
        var center = worldPos.ToModuleLocation(transform, modules.ModuleSize);
        var diff = CalculateLoadSet(center, renderDistance, modules.Modules, context.Generator);

        UnloadModules(diff.ToUnload, modules.Modules);
        return await LoadModulesAsync(diff.ToLoad, modules.Modules, context);
    }

    public async Task<IEnumerable<Task<ModuleGenerationResponse>>> BuildModuleMeshes(

    )
    {

    }

    private Task<ModuleGenerationResponse> GenerateAsync(
        BlockStore blockStore,
        Material moduleMaterial,
        ModuleLocation location,
        ModuleLoadContext context)
    {
        return Task.Run(() =>
        {
            var response = context.Generator.GenerateModules(
                location,
                context.ModuleMaterial);

            var cacheCopy = context.SurfaceCache.ExposedSurfaces
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToHashSet());

            var cache = new ExposedSurfaceCache(cacheCopy);
            response.SurfaceCache = cache;

            foreach (var module in response.GeneratedModules)
            {
                if (!module.Value.HasBlocks) continue;
                var generationResponse = ModuleMeshGenerator.BuildModuleMesh(
                    cache,
                    module.Value,
                    location,
                    moduleMaterial,
                    blockStore
                );
            }

            return response;
        });
    }

    private Task<ModuleGenerationResponse> GenerateAsync(
    BlockStore blockStore,
    Material moduleMaterial,
    ModuleLocation location,
    ModuleLoadContext context)
    {
        return Task.Run(() =>
        {
            var response = context.Generator.GenerateModules(
                location,
                context.ModuleMaterial);

            var cacheCopy = context.SurfaceCache.ExposedSurfaces
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToHashSet());

            var cache = new ExposedSurfaceCache(cacheCopy);
            response.SurfaceCache = cache;

            foreach (var module in response.GeneratedModules)
            {
                if (!module.Value.HasBlocks) continue;
                var generationResponse = ModuleMeshGenerator.BuildModuleMesh(
                    cache,
                    module.Value,
                    location,
                    moduleMaterial,
                    blockStore
                );
            }

            return response;
        });
    }


    public ExposedSurfaceCache BuildMesh(
        ExposedSurfaceCache cache,
        BlockStore blockStore,
        ModuleLocation moduleLocation
    )
    {
        if (!HasBlocks)
        {
            return null;
        }

        var response = ModuleMeshGenerator.BuildModuleMesh(
            cache,
            this,
            moduleLocation,
            moduleMaterial,
            blockStore
        );

        Mesh = response.Mesh;
        return response.Cache;
    }

    private async Task<IEnumerable<Task<ModuleGenerationResponse>>> LoadModulesAsync(
        List<ModuleLocation> positions,
        Dictionary<ModuleLocation, Module> loaded,
        ModuleLoadContext context
    )
    {
        using var semaphore = new SemaphoreSlim(MaxConcurrentModuleLoads);

        var tasks = positions.Select(async pos =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await GenerateAsync(pos, context);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return tasks;
    }

    private void UnloadModules(
        IEnumerable<ModuleLocation> positions,
        Dictionary<ModuleLocation, Module> loaded
    )
    {
        foreach (var pos in positions)
        {
            if (loaded.Remove(pos, out var module))
                module.QueueFree();
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