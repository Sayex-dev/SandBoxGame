using System.Threading.Tasks;
using Godot;

public class ConstructLoadingController
{
    public bool isFullyLoaded = false;
    private readonly ConstructData data;
    private readonly ConstructModuleBuilder moduleBuilder;
    private readonly ConstructVisualsController visuals;
    private readonly ConstructGenerator generator;

    public ConstructLoadingController(
        ConstructData data,
        ConstructModuleBuilder moduleBuilder,
        ConstructVisualsController visuals,
        ConstructGenerator generator)
    {
        this.data = data;
        this.moduleBuilder = moduleBuilder;
        this.visuals = visuals;
        this.generator = generator;
    }

    public async Task UpdateLoading(WorldGridPos worldPos, int renderDistance, int simulationDistance)
    {
        await LoadAround(worldPos, simulationDistance);

        // Lazy loading of modules
        //for (int i = simulationDistance; i < renderDistance; i++)
        //{
        //	if (!loadComplete)
        //		return;
        //	await LoadAround(worldPos, i);
        //}
    }

    public async Task LoadAround(WorldGridPos worldPos, int loadDistance)
    {
        var context = new ModuleLoadContext(
            data.Modules.ModuleSize,
            data.BlockStore,
            data.ModuleMaterial,
            generator
        );
        var generationResponse = moduleBuilder.GenerateModulesAround(
            worldPos, loadDistance, data.Transform, data.Modules, context);

        // Load new modules
        foreach (Task<GenerateModulesResponse> task in generationResponse.GenerationTaskHandles)
        {
            var response = await task;
            foreach (var kvp in response.GeneratedModules)
            {
                ModuleLocation moduleLocation = kvp.Key;
                Module module = kvp.Value;
                Mesh mesh = response.Meshes[moduleLocation];

                // Update bounds
                data.Bounds.AddPosition(module.MinPos.ToConstruct(moduleLocation, module.ModuleSize));
                data.Bounds.AddPosition(module.MaxPos.ToConstruct(moduleLocation, module.ModuleSize));

                // Update modules
                data.Modules.Add(moduleLocation, module);

                // Update visuals
                visuals.AddModule(moduleLocation, mesh);
            }
        }

        // Unload modules that are out of range
        bool needsBoundsRebuild = false;
        foreach (ModuleLocation moduleLocation in generationResponse.ToUnload)
        {
            // Remove from modules
            if (data.Modules.Remove(moduleLocation, out Module module))
            {
                // Update bounds if necessary
                if (module.HasBlocks)
                {
                    ConstructGridPos minPos = module.MinPos.ToConstruct(moduleLocation, module.ModuleSize);
                    ConstructGridPos maxPos = module.MaxPos.ToConstruct(moduleLocation, module.ModuleSize);

                    if (data.Bounds.IsOnBounds(minPos) || data.Bounds.IsOnBounds(maxPos))
                    {
                        needsBoundsRebuild = true;
                    }
                }
            }

            // Remove visuals
            visuals.RemoveModule(moduleLocation);
        }

        if (needsBoundsRebuild)
        {
            // Rebuild bounds since we removed a module on the boundary
            data.Bounds.Clear();
            foreach (var kvp in data.Modules.Modules)
            {
                var remainingModule = kvp.Value;
                var remainingLocation = kvp.Key;
                if (remainingModule.HasBlocks)
                {
                    data.Bounds.AddPosition(remainingModule.MinPos.ToConstruct(remainingLocation, remainingModule.ModuleSize));
                    data.Bounds.AddPosition(remainingModule.MaxPos.ToConstruct(remainingLocation, remainingModule.ModuleSize));
                }
            }
        }
    }
}
