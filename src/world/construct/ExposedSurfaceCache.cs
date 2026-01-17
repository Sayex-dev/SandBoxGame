using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class ExposedSurfaceCache
{
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

    public ExposedSurfaceCache()
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            exposedSurfaces[dir] = new HashSet<ConstructGridPos>();
        }
    }

    public void SetupConstruct(Construct construct)
    {
        exposedSurfaces.Clear();
        Dictionary<ModuleLocation, Module> modules = construct.GetModules();
        foreach (KeyValuePair<ModuleLocation, Module> kvp in modules)
        {
            Module module = kvp.Value;
            ModuleLocation moduleLocation = kvp.Key;

            AddModule(construct, module, moduleLocation);
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

    public void AddModule(Construct construct, Module module, ModuleLocation moduleLocation)
    {
        int[] blocks = module.GetBlockArray();

        for (int i = 0; i < blocks.Length; i++)
        {
            int blockId = blocks[i];
            if (blockId == -1) continue;
            ModuleGridPos modulePos = module.ArrayToInModulePos(i);
            ConstructGridPos constructPos = modulePos.ToConstruct(moduleLocation, construct.ModuleSize);
            AddBlock(construct, constructPos);
        }
    }

    public void AddBlock(Construct construct, ConstructGridPos constructPos)
    {
        Dictionary<Direction, bool> collideableDirections = GetExposedDirections(construct, constructPos);
        foreach (KeyValuePair<Direction, bool> kvp in collideableDirections)
        {
            Direction dir = kvp.Key;
            bool isExposed = kvp.Value;
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

    public void RemoveBlock(Construct construct, ConstructGridPos constructPos)
    {
        Dictionary<Direction, bool> exposedDirs = GetExposedDirections(construct, constructPos);
        foreach (KeyValuePair<Direction, bool> kvp in exposedDirs)
        {
            Direction dir = kvp.Key;
            bool isExposed = kvp.Value;

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

    private Dictionary<Direction, bool> GetExposedDirections(Construct construct, ConstructGridPos constructPos)
    {
        Dictionary<Direction, bool> exposedDirections = [];
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            WorldGridPos checkPos = new(constructPos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
            int checkBlockId;
            if (construct.HasBlock(checkPos, out checkBlockId)) continue;
            exposedDirections[dir] = checkBlockId != -1;
        }
        return exposedDirections;
    }
}