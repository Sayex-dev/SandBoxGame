using System.Threading.Tasks;

/// <summary>
/// Loads all required modules for a finite construct once.
/// Used for preset/bounded constructs that have a known, fixed set of modules.
/// After loading completes, this loader is no longer needed.
/// </summary>
public static class ConstructOneTimeLoader
{
    /// <summary>
    /// Loads all modules for the construct by querying the generator for its required modules.
    /// Returns once all modules have been generated and integrated.
    /// </summary>
    public static async Task LoadAll(
        ConstructData data,
        ConstructModuleBuilder moduleBuilder,
        ConstructVisualsController visuals,
        ConstructGenerator generator)
    {
        var context = new ModuleLoadContext(
            data.Modules.ModuleSize,
            data.BlockStore,
            data.ModuleMaterial,
            generator
        );

        var generationTasks = moduleBuilder.GenerateAllModules(context);

        // Load all modules
        await ModuleIntegrationHelper.IntegrateGeneratedModules(
            generationTasks, data, visuals);
    }
}
