using Godot;  
using System;  
using System.Linq;  
using System.Collections.Generic;  
  
public class ExposedModuleSurfaceCache  
{  
    private Dictionary<Direction, Dictionary<ModuleGridPos, BlockSurfaceInfo>> exposedSurfaces = new();  
      
    private Module module;  
    private BlockStore blockStore;  
      
    public struct BlockSurfaceInfo  
    {  
        public int BlockId;  
        public Direction BlockDirection;  
        public Orientation BlockOrientation;  
        public BlockDefault BlockDefault;  
          
        public BlockSurfaceInfo(Block block, BlockStore store)  
        {  
            BlockId = block.Id;  
            BlockDirection = block.Direction;  
            BlockOrientation = block.Orientation;  
              
            BlockDefault = store.GetBlockDefault(block);  
        }  
    }  
      
    public ExposedModuleSurfaceCache(Module module, BlockStore blockStore)  
    {  
        this.module = module;  
        this.blockStore = blockStore;  
          
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))  
        {  
            exposedSurfaces[dir] = new Dictionary<ModuleGridPos, BlockSurfaceInfo>();  
        }  
    }  
      
    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ModuleGridPos>> GetExposedSurfaces(  
        Func<BlockSurfaceInfo, bool> filter = null)  
    {  
        var result = new Dictionary<Direction, IReadOnlyCollection<ModuleGridPos>>();  
          
        foreach (var kvp in exposedSurfaces)  
        {  
            Direction dir = kvp.Key;  
            var positions = filter == null  
                ? kvp.Value.Keys.ToHashSet()  
                : kvp.Value.Where(x => filter(x.Value)).Select(x => x.Key).ToHashSet();  
              
            if (positions.Count > 0)  
                result[dir] = positions;  
        }  
          
        return result;  
    }

    public IReadOnlyDictionary<Direction, IReadOnlyDictionary<ModuleGridPos, BlockSurfaceInfo>> GetExposedSurfacesWithInfo(  
        Func<BlockSurfaceInfo, bool> filter = null)  
    {  
        var result = new Dictionary<Direction, IReadOnlyDictionary<ModuleGridPos, BlockSurfaceInfo>>();  
          
        foreach (var kvp in exposedSurfaces)  
        {  
            Direction dir = kvp.Key;  
            var filtered = filter == null  
                ? kvp.Value.ToDictionary(x => x.Key, x => x.Value)  
                : kvp.Value.Where(x => filter(x.Value)).ToDictionary(x => x.Key, x => x.Value);  
              
            if (filtered.Count > 0)  
                result[dir] = filtered;  
        }  
          
        return result;  
    }

    public bool TryGetBlockInfo(Direction direction, ModuleGridPos position, out BlockSurfaceInfo info)
    {
        return exposedSurfaces[direction].TryGetValue(position, out info);
    }
      
    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ModuleGridPos>> GetVoxelSurfaces()  
    {  
        return GetExposedSurfaces(info => info.BlockDefault.RenderType == BlockRenderType.Voxel);  
    }

    public IReadOnlyDictionary<Direction, IReadOnlyDictionary<ModuleGridPos, BlockSurfaceInfo>> GetVoxelSurfacesWithInfo()  
    {  
        return GetExposedSurfacesWithInfo(info => info.BlockDefault.RenderType == BlockRenderType.Voxel);  
    }
      
    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ModuleGridPos>> GetCollisionSurfaces()  
    {  
        return GetExposedSurfaces(info =>  
            info.BlockDefault.RenderType == BlockRenderType.Voxel ||  
            info.BlockDefault.RenderType == BlockRenderType.Model);  
    }

    // NEW: With info
    public IReadOnlyDictionary<Direction, IReadOnlyDictionary<ModuleGridPos, BlockSurfaceInfo>> GetCollisionSurfacesWithInfo()  
    {  
        return GetExposedSurfacesWithInfo(info =>  
            info.BlockDefault.RenderType == BlockRenderType.Voxel ||  
            info.BlockDefault.RenderType == BlockRenderType.Model);  
    }
      
    public List<ModuleGridPos> GetModelPositions()  
    {  
        var modelPositions = new HashSet<ModuleGridPos>();  
          
        foreach (var surfaces in exposedSurfaces.Values)  
        {  
            foreach (var kvp in surfaces)  
            {  
                if (kvp.Value.BlockDefault.RenderType == BlockRenderType.Model)  
                    modelPositions.Add(kvp.Key);  
            }  
        }  
          
        return modelPositions.ToList();  
    }

    public List<(ModuleGridPos pos, BlockSurfaceInfo info)> GetModelPositionsWithInfo()  
    {  
        var modelData = new Dictionary<ModuleGridPos, BlockSurfaceInfo>();  
          
        foreach (var surfaces in exposedSurfaces.Values)  
        {  
            foreach (var kvp in surfaces)  
            {  
                if (kvp.Value.BlockDefault.RenderType == BlockRenderType.Model)  
                    modelData[kvp.Key] = kvp.Value;  
            }  
        }  
          
        return modelData.Select(x => (x.Key, x.Value)).ToList();  
    }
      
    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ModuleGridPos>> GetAllExposedSurfaces()  
    {  
        return GetExposedSurfaces(null);
    }

    public IReadOnlyDictionary<Direction, IReadOnlyDictionary<ModuleGridPos, BlockSurfaceInfo>> GetAllExposedSurfacesWithInfo()  
    {  
        return GetExposedSurfacesWithInfo(null);
    }
  
    public void RebuildModule()  
    {  
        // Clear existing data  
        foreach (var surfaces in exposedSurfaces.Values)  
        {  
            surfaces.Clear();  
        }  
  
        // Empty module case  
        if (module.BlockCount == 0)  
            return;  
  
        // Filled module case  
        int size = module.ModuleSize;  
        if (module.BlockCount == Math.Pow(size, 3))  
        {  
            foreach (var kvp in exposedSurfaces)  
            {  
                Direction dir = kvp.Key;  
                var surfaceDict = kvp.Value;  
  
                for (int u = 0; u < size; u++)  
                {  
                    for (int v = 0; v < size; v++)  
                    {  
                        Vector3I position = GetSurfacePosition(dir, u, v, size);  
                        Block block = module.GetBlock(position);  
                        surfaceDict[position] = new BlockSurfaceInfo(block, blockStore);  
                    }  
                }  
            }  
            return;  
        }  
  
        // Default case - iterate all blocks  
        Block[] blocks = module.GetBlockArray();  
        for (int i = 0; i < blocks.Length; i++)  
        {  
            Block block = blocks[i];  
            if (block.IsEmpty)  
                continue;  
              
            ModuleGridPos modulePos = module.ArrayToInModulePos(i);  
            AddBlock(modulePos, block);  
        }  
    }  
  
    public void AddBlock(ModuleGridPos modulePos, Block block)  
    {  
        BlockSurfaceInfo blockInfo = new BlockSurfaceInfo(block, blockStore);  
          
        // Check each face of the added block  
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))  
        {  
            ModuleGridPos neighborPos = new(modulePos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));  
            Block neighbor = module.GetBlock(neighborPos);  
              
            if (neighbor.IsEmpty)  
            {  
                // This face is exposed  
                exposedSurfaces[dir][modulePos] = blockInfo;  
            }  
            else  
            {  
                // This face is occluded - remove neighbor's opposite face  
                Direction oppositeDir = DirectionTools.Invert(dir);  
                exposedSurfaces[oppositeDir].Remove(neighborPos);  
            }  
        }  
    }  
  
    public void RemoveBlock(ModuleGridPos modulePos)  
    {  
        // Remove all faces of this block  
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))  
        {  
            exposedSurfaces[dir].Remove(modulePos);  
        }  
          
        // Expose neighbors' faces that were hidden by this block  
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))  
        {  
            ModuleGridPos neighborPos = new(modulePos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));  
            Block neighbor = module.GetBlock(neighborPos);  
              
            if (!neighbor.IsEmpty)  
            {  
                // Neighbor's face toward removed block is now exposed  
                Direction oppositeDir = DirectionTools.Invert(dir);  
                BlockSurfaceInfo neighborInfo = new BlockSurfaceInfo(neighbor, blockStore);  
                exposedSurfaces[oppositeDir][neighborPos] = neighborInfo;  
            }  
        }  
    }  
  
    private Vector3I GetSurfacePosition(Direction direction, int u, int v, int size)  
    {  
        return direction switch  
        {  
            Direction.FORWARD => new Vector3I(u, v, 0),  
            Direction.BACKWARD => new Vector3I(u, v, size - 1),  
            Direction.RIGHT => new Vector3I(size - 1, u, v),  
            Direction.LEFT => new Vector3I(0, u, v),  
            Direction.UP => new Vector3I(u, size - 1, v),  
            Direction.DOWN => new Vector3I(u, 0, v),  
            _ => throw new ArgumentException($"Invalid direction: {direction}")  
        };  
    }  
}