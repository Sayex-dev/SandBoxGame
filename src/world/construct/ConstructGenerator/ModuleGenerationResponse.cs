using System.Collections.Generic;

public class ModuleBlockGenerationResponse
{
    public bool GeneratedAllModules = false;
    public Dictionary<ModuleLocation, Module> GeneratedModules = [];
}