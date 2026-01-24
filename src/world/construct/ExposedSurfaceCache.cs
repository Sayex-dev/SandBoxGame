using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class ExposedSurfaceCache
{
    public ConstructGridPos MinPos { get; private set; }
    public ConstructGridPos MaxPos { get; private set; }

    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ConstructGridPos>> ExposedSurfaces
    {
        get
        {
            return exposedSurfaces.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyCollection<ConstructGridPos>)kvp.Value
            );
        }
    }
    private Dictionary<Direction, HashSet<ConstructGridPos>> exposedSurfaces = new();

    public ExposedSurfaceCache(Dictionary<Direction, HashSet<ConstructGridPos>> exposedSurfaces = null)
    {
        if (exposedSurfaces == null)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                this.exposedSurfaces[dir] = new HashSet<ConstructGridPos>();
            }
        }
        else
        {
            this.exposedSurfaces = exposedSurfaces;
        }
    }

    public void SetupConstruct(Construct construct)
    {
        exposedSurfaces.Clear();
        Dictionary<ModuleLocation, Module> modules = construct.Modules.Modules;
        if (modules.Count == 0) return;
        foreach (KeyValuePair<ModuleLocation, Module> kvp in modules)
        {
            Module module = kvp.Value;
            ModuleLocation moduleLocation = kvp.Key;

            AddModule(module, moduleLocation);
        }
    }

    public void CombineWith(ExposedSurfaceCache other)
    {
        foreach (KeyValuePair<Direction, IReadOnlyCollection<ConstructGridPos>> kvp in other.ExposedSurfaces)
        {
            Direction dir = kvp.Key;
            Direction inverted = DirectionTools.Invert(dir);
            Vector3I offset = (Vector3I)DirectionTools.GetWorldDirVec(dir);

            HashSet<ConstructGridPos> mySet = exposedSurfaces[dir];
            HashSet<ConstructGridPos> myOppositeSet = exposedSurfaces[inverted];

            foreach (ConstructGridPos pos in kvp.Value)
            {
                ConstructGridPos neighbor =
                    new ConstructGridPos(pos.Value + offset);

                if (!myOppositeSet.Remove(neighbor)) mySet.Add(pos);
            }
        }
    }

    public void AddModule(Module module, ModuleLocation moduleLocation)
    {
        if (module.BlockCount == 0) return;

        int[] blocks = module.GetBlockArray();

        for (int i = 0; i < blocks.Length; i++)
        {
            int blockId = blocks[i];
            if (blockId == -1) continue;
            ModuleGridPos modulePos = module.ArrayToInModulePos(i);
            ConstructGridPos constructPos = modulePos.ToConstruct(moduleLocation, module.ModuleSize);
            AddBlock(constructPos);
        }
    }

    public void AddBlock(ConstructGridPos constructPos)
    {
        bool[] exposedDirs = GetExposedDirections(constructPos);
        for (int i = 0; i < 6; i++)
        {
            Direction dir = (Direction)i;
            bool isExposed = exposedDirs[i];

            if (isExposed)
            {
                exposedSurfaces[dir].Add(constructPos);
            }
            else
            {
                ConstructGridPos removePos = new(constructPos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
                Direction invertedDir = DirectionTools.Invert(dir);
                if (exposedSurfaces[invertedDir].Contains(removePos))
                    exposedSurfaces[invertedDir].Remove(removePos);
            }
        }
    }

    public void RemoveBlock(ConstructGridPos constructPos)
    {
        bool[] exposedDirs = GetExposedDirections(constructPos);
        for (int i = 0; i < 6; i++)
        {
            Direction dir = (Direction)i;
            bool isExposed = exposedDirs[i];

            if (!isExposed)
            {
                ConstructGridPos toExposePos = new(constructPos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
                Direction invertedDir = DirectionTools.Invert(dir);
                exposedSurfaces[invertedDir].Add(toExposePos);
            }
            else
            {
                exposedSurfaces[dir].Remove(constructPos);
            }
        }
    }

    private bool[] GetExposedDirections(ConstructGridPos constructPos)
    {
        bool[] exposedDirections = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            Direction dir = (Direction)i;
            ConstructGridPos checkPos = new(constructPos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
            Direction invertedDir = DirectionTools.Invert(dir);
            bool isExposed = !exposedSurfaces[invertedDir].Contains(checkPos);
            exposedDirections[i] = isExposed;
        }
        return exposedDirections;
    }
}