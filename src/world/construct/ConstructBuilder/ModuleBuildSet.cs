using System.Collections.Generic;

public sealed class ModuleBuildSet
{
    public List<ModuleLocation> ToLoad { get; } = new();
    public List<ModuleLocation> ToUnload { get; } = new();
}