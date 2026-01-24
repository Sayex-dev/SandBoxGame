using System.Collections.Generic;

public sealed class ModuleLoadSet
{
    public List<ModuleLocation> ToLoad { get; } = new();
    public List<ModuleLocation> ToUnload { get; } = new();
}