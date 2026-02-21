using System.Threading.Tasks;

/// <summary>
/// Continuously loads and unloads modules around a moving position.
/// Used for the global world construct that streams terrain as the player moves.
/// </summary>
public class ConstructStreamingLoader
{
    private readonly ConstructData data;
    private readonly ConstructModuleBuilder moduleBuilder;
    private readonly ConstructVisualsController visuals;
    private readonly ConstructGenerator generator;

    public ConstructStreamingLoader(
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
    }

    private async Task LoadAround(WorldGridPos worldPos, int loadDistance)
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
        await ModuleIntegrationHelper.IntegrateGeneratedModules(
            generationResponse.GenerationTaskHandles, data, visuals);

        // Unload modules that are out of range
        ModuleIntegrationHelper.UnloadModules(
            generationResponse.ToUnload, data, visuals);
    }
}
