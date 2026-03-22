using System;
using System.Linq;
using System.Collections.Generic;

public class SurfaceCache<T> where T : BlockDefault
{
    private Dictionary<Direction, Dictionary<ModuleGridPos, BlockSurfaceInfo>> exposedSurfaces = new();
    
    public struct BlockSurfaceInfo
    {
        public Block Block;
        public T BlockDefault;
        
        public BlockSurfaceInfo(Block block, T blockDefault)
        {
            Block = block;
            BlockDefault = blockDefault;
        }
    }
    
    public SurfaceCache()
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            exposedSurfaces[dir] = new Dictionary<ModuleGridPos, BlockSurfaceInfo>();
        }
    }
    
    // Internal methods for builder
    internal void AddExposedFace(Direction dir, ModuleGridPos pos, BlockSurfaceInfo info)
    {
        exposedSurfaces[dir][pos] = info;
    }
    
    internal void RemoveExposedFace(Direction dir, ModuleGridPos pos)
    {
        exposedSurfaces[dir].Remove(pos);
    }
    
    internal void Clear()
    {
        foreach (var dict in exposedSurfaces.Values)
            dict.Clear();
    }
    
    // Public query methods
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
    
    public IReadOnlyDictionary<Direction, IReadOnlyCollection<ModuleGridPos>> GetAllExposedSurfaces()
    {
        return GetExposedSurfaces(null);
    }
    
    public IReadOnlyDictionary<Direction, IReadOnlyDictionary<ModuleGridPos, BlockSurfaceInfo>> GetAllExposedSurfacesWithInfo()
    {
        return GetExposedSurfacesWithInfo(null);
    }
    
    // Get positions as list (useful for model placement)
    public List<ModuleGridPos> GetAllPositions()
    {
        var positions = new HashSet<ModuleGridPos>();
        
        foreach (var surfaces in exposedSurfaces.Values)
        {
            foreach (var kvp in surfaces)
            {
                positions.Add(kvp.Key);
            }
        }
        
        return positions.ToList();
    }
    
    public List<(ModuleGridPos pos, BlockSurfaceInfo info)> GetAllPositionsWithInfo()
    {
        var data = new Dictionary<ModuleGridPos, BlockSurfaceInfo>();
        
        foreach (var surfaces in exposedSurfaces.Values)
        {
            foreach (var kvp in surfaces)
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        return data.Select(x => (x.Key, x.Value)).ToList();
    }
    
    // Get count for debugging/stats
    public int GetTotalSurfaceCount()
    {
        return exposedSurfaces.Values.Sum(dict => dict.Count);
    }
    
    public int GetSurfaceCount(Direction direction)
    {
        return exposedSurfaces[direction].Count;
    }
}