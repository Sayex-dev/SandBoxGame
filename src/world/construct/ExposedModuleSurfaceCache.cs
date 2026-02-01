using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class ExposedModuleSurfaceCache
{
    public ModuleGridPos MinPos { get; private set; }
    public ModuleGridPos MaxPos { get; private set; }

    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ModuleGridPos>> ExposedSurfaces
    {
        get
        {
            return exposedSurfaces.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyCollection<ModuleGridPos>)kvp.Value
            );
        }
    }
    private Dictionary<Direction, HashSet<ModuleGridPos>> exposedSurfaces = new();

    public ExposedModuleSurfaceCache(Dictionary<Direction, HashSet<ModuleGridPos>> exposedSurfaces = null)
    {
        if (exposedSurfaces == null)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                this.exposedSurfaces[dir] = new HashSet<ModuleGridPos>();
            }
        }
        else
        {
            this.exposedSurfaces = exposedSurfaces;
        }
    }

    public void RebuildModule(Module module)
    {
        // Empty module case
        if (module.BlockCount == 0) return;

        // Filled module case
        int size = module.ModuleSize;
        if (module.BlockCount == Math.Pow(size, 3))
        {
            foreach (var kvp in exposedSurfaces)
            {
                Direction dir = kvp.Key;
                HashSet<ModuleGridPos> surfaceSet = kvp.Value;

                for (int u = 0; u < size; u++)
                {
                    for (int v = 0; v < size; v++)
                    {
                        Vector3I position = GetSurfacePosition(dir, u, v, size);
                        surfaceSet.Add(position);
                    }
                }
            }
        }

        int[] blocks = module.GetBlockArray();

        for (int i = 0; i < blocks.Length; i++)
        {
            int blockId = blocks[i];
            if (blockId == -1) continue;
            ModuleGridPos modulePos = module.ArrayToInModulePos(i);
            AddBlock(modulePos);
        }
    }

    public void AddBlock(ModuleGridPos modulePos)
    {
        bool[] exposedDirs = GetExposedDirections(modulePos);
        for (int i = 0; i < 6; i++)
        {
            Direction dir = (Direction)i;
            bool isExposed = exposedDirs[i];

            if (isExposed)
            {
                exposedSurfaces[dir].Add(modulePos);
            }
            else
            {
                ModuleGridPos removePos = new(modulePos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
                Direction invertedDir = DirectionTools.Invert(dir);
                if (exposedSurfaces[invertedDir].Contains(removePos))
                    exposedSurfaces[invertedDir].Remove(removePos);
            }
        }
    }

    public void RemoveBlock(ModuleGridPos modulePos)
    {
        bool[] exposedDirs = GetExposedDirections(modulePos);
        for (int i = 0; i < 6; i++)
        {
            Direction dir = (Direction)i;
            bool isExposed = exposedDirs[i];

            if (!isExposed)
            {
                ModuleGridPos toExposePos = new(modulePos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
                Direction invertedDir = DirectionTools.Invert(dir);
                exposedSurfaces[invertedDir].Add(toExposePos);
            }
            else
            {
                exposedSurfaces[dir].Remove(modulePos);
            }
        }
    }

    private bool[] GetExposedDirections(ModuleGridPos modulePos)
    {
        bool[] exposedDirections = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            Direction dir = (Direction)i;
            ModuleGridPos checkPos = new(modulePos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
            Direction invertedDir = DirectionTools.Invert(dir);
            bool isExposed = !exposedSurfaces[invertedDir].Contains(checkPos);
            exposedDirections[i] = isExposed;
        }
        return exposedDirections;
    }

    private Vector3I GetSurfacePosition(Direction direction, int u, int v, int size)
    {
        return direction switch
        {
            Direction.FORWARD => new Vector3I(u, v, size - 1),
            Direction.BACKWARD => new Vector3I(u, v, 0),
            Direction.RIGHT => new Vector3I(size - 1, u, v),
            Direction.LEFT => new Vector3I(0, u, v),
            Direction.UP => new Vector3I(u, size - 1, v),
            Direction.DOWN => new Vector3I(u, 0, v),
            _ => throw new ArgumentException($"Invalid direction: {direction}")
        };
    }
}