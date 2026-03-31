using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages both render and collision surface caches for a module.
/// Handles building, incremental updates, and provides access to cached surfaces.
/// </summary>
public class SurfaceCacheController
{
    private static readonly Direction[] Directions = (Direction[])Enum.GetValues(typeof(Direction));

    // The managed caches
    private readonly SurfaceCache<VoxelBlockDefault> renderCache = new();
    private readonly SurfaceCache<BlockDefault> collisionCache = new();

    // Cached block data to avoid repeated lookups
    private struct BlockData
    {
        public ModuleGridPos Pos;
        public Block Block;
        public BlockDefault BlockDefault;
        public VoxelBlockDefault VoxelDefault; // null if not voxel
        public bool IsVoxel;
        public bool IsModel;
    }

    // Public accessors
    public SurfaceCache<VoxelBlockDefault> RenderCache => renderCache;
    public SurfaceCache<BlockDefault> CollisionCache => collisionCache;

    /// <summary>
    /// Completely rebuilds both caches from the module data.
    /// Use this for initial load or when making bulk changes.
    /// </summary>
    public void RebuildModule(Module module)
    {
        renderCache.Clear();
        collisionCache.Clear();

        // Handle empty module
        if (module.BlockCount == 0)
            return;

        int size = GameSettings.Instance.ModuleSize;

        // Optimization: fully filled module
        if (module.BlockCount == Math.Pow(size, 3))
        {
            BuildFullModuleCaches(module, size);
            return;
        }

        // Default case: sparse module
        BuildSparseCaches(module);
    }

    /// <summary>
    /// Incrementally updates caches when a block is added to the module.
    /// Much faster than full rebuild for single block changes.
    /// </summary>
    public void AddBlock(Module module, ModuleGridPos pos, Block block)
    {
        BlockDefault blockDefault = BlockStore.Instance.GetBlockDefault(block);
        VoxelBlockDefault voxelDefault = blockDefault as VoxelBlockDefault;
        ModelBlockDefault modelDefault = blockDefault as ModelBlockDefault;

        bool isVoxel = voxelDefault != null;
        bool isModel = modelDefault != null;

        if (!isVoxel && !isModel)
            return;

        foreach (Direction dir in Directions)
        {
            ModuleGridPos neighborPos = new(pos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
            bool hasBlock = module.HasBlock(neighborPos, out Block neighbor);

            if (neighbor.IsEmpty || !hasBlock)
            {
                // Add this block's exposed faces
                if (isVoxel)
                {
                    var renderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                        block, voxelDefault);
                    renderCache.AddExposedFace(dir, pos, renderInfo);
                }

                if (isVoxel || isModel)
                {
                    var collisionInfo = new SurfaceCache<BlockDefault>.BlockSurfaceInfo(
                        block, blockDefault);
                    collisionCache.AddExposedFace(dir, pos, collisionInfo);
                }
            }
            else
            {
                // Neighbor exists - handle occlusion
                BlockDefault neighborDefault = BlockStore.Instance.GetBlockDefault(neighbor);
                VoxelBlockDefault neighborVoxelDefault = neighborDefault as VoxelBlockDefault;

                bool neighborIsVoxel = neighborVoxelDefault != null;
                bool neighborIsModel = neighborDefault is ModelBlockDefault;

                Direction oppositeDir = DirectionTools.Invert(dir);

                // RENDER CACHE LOGIC
                if (isVoxel && !neighborIsVoxel)
                {
                    // This voxel face is visible (neighbor is model or empty)
                    var renderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                        block, voxelDefault);
                    renderCache.AddExposedFace(dir, pos, renderInfo);
                }

                if (neighborIsVoxel && !isVoxel)
                {
                    // Neighbor voxel face becomes visible (this is model)
                    var neighborRenderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                        neighbor, neighborVoxelDefault);
                    renderCache.AddExposedFace(oppositeDir, neighborPos, neighborRenderInfo);
                }

                if (isVoxel && neighborIsVoxel)
                {
                    // Both voxel - remove neighbor's face (occluded)
                    renderCache.RemoveExposedFace(oppositeDir, neighborPos);
                }

                // COLLISION CACHE LOGIC
                if (neighborIsVoxel || neighborIsModel)
                {
                    // Neighbor is solid - remove its face toward us (now occluded)
                    collisionCache.RemoveExposedFace(oppositeDir, neighborPos);
                }
            }
        }
    }

    /// <summary>
    /// Incrementally updates caches when a block is removed from the module.
    /// Much faster than full rebuild for single block changes.
    /// </summary>
    public void RemoveBlock(Module module, ModuleGridPos pos)
    {
        // Remove all faces of the removed block from both caches
        foreach (Direction dir in Directions)
        {
            renderCache.RemoveExposedFace(dir, pos);
            collisionCache.RemoveExposedFace(dir, pos);
        }

        // Expose neighbor faces that were previously hidden
        foreach (Direction dir in Directions)
        {
            ModuleGridPos neighborPos = new(pos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
            Block neighbor = module.GetBlock(neighborPos);

            if (neighbor.IsEmpty)
                continue;

            BlockDefault neighborDefault = BlockStore.Instance.GetBlockDefault(neighbor);
            VoxelBlockDefault neighborVoxelDefault = neighborDefault as VoxelBlockDefault;

            bool neighborIsVoxel = neighborVoxelDefault != null;
            bool neighborIsModel = neighborDefault is ModelBlockDefault;

            Direction oppositeDir = DirectionTools.Invert(dir);

            // Expose neighbor's face toward removed block (now facing air)
            if (neighborIsVoxel)
            {
                var renderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                    neighbor, neighborVoxelDefault);
                renderCache.AddExposedFace(oppositeDir, neighborPos, renderInfo);
            }

            if (neighborIsVoxel || neighborIsModel)
            {
                var collisionInfo = new SurfaceCache<BlockDefault>.BlockSurfaceInfo(
                    neighbor, neighborDefault);
                collisionCache.AddExposedFace(oppositeDir, neighborPos, collisionInfo);
            }
        }
    }

    /// <summary>
    /// Clears both caches. Useful for cleanup or before rebuild.
    /// </summary>
    public void Clear()
    {
        renderCache.Clear();
        collisionCache.Clear();
    }

    /// <summary>
    /// Gets statistics about the caches for debugging.
    /// </summary>
    public (int renderSurfaces, int collisionSurfaces) GetStats()
    {
        return (renderCache.GetTotalSurfaceCount(), collisionCache.GetTotalSurfaceCount());
    }

    // Private build methods

    private void BuildSparseCaches(Module module)
    {
        Block[] blocks = module.GetBlockArray();

        // PHASE 1: Collect all block data with single lookup per block
        var blockDataDict = new Dictionary<ModuleGridPos, BlockData>();

        for (int i = 0; i < blocks.Length; i++)
        {
            Block block = blocks[i];
            if (block.IsEmpty)
                continue;

            ModuleGridPos pos = module.ArrayToInModulePos(i);
            BlockDefault blockDefault = BlockStore.Instance.GetBlockDefault(block);

            var data = new BlockData
            {
                Pos = pos,
                Block = block,
                BlockDefault = blockDefault,
                VoxelDefault = blockDefault as VoxelBlockDefault,
                IsVoxel = blockDefault is VoxelBlockDefault,
                IsModel = blockDefault is ModelBlockDefault
            };

            // Only track blocks that matter for either cache
            if (data.IsVoxel || data.IsModel)
                blockDataDict[pos] = data;
        }

        // PHASE 2: Build surface caches using collected data
        foreach (var blockData in blockDataDict.Values)
        {
            foreach (Direction dir in Directions)
            {
                ModuleGridPos neighborPos = new(blockData.Pos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));

                // Check if neighbor exists in our tracked blocks
                bool hasNeighbor = blockDataDict.TryGetValue(neighborPos, out var neighborData);

                if (!hasNeighbor)
                {
                    // No solid neighbor = exposed to air
                    if (blockData.IsVoxel)
                    {
                        var renderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                            blockData.Block, blockData.VoxelDefault);
                        renderCache.AddExposedFace(dir, blockData.Pos, renderInfo);
                    }

                    if (blockData.IsVoxel || blockData.IsModel)
                    {
                        var collisionInfo = new SurfaceCache<BlockDefault>.BlockSurfaceInfo(
                            blockData.Block, blockData.BlockDefault);
                        collisionCache.AddExposedFace(dir, blockData.Pos, collisionInfo);
                    }
                }
                else
                {
                    // Has solid neighbor - apply occlusion rules

                    // RENDER: Voxel surfaces are visible if neighbor is NOT voxel
                    // (voxel-model boundary shows voxel face, model-model hides both)
                    if (blockData.IsVoxel && !neighborData.IsVoxel)
                    {
                        var renderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                            blockData.Block, blockData.VoxelDefault);
                        renderCache.AddExposedFace(dir, blockData.Pos, renderInfo);
                    }

                    // COLLISION: All solid-solid interfaces are internal (occluded)
                    // So we don't add anything to collision cache when both are solid
                }
            }
        }
    }

    private void BuildFullModuleCaches(Module module, int size)
    {
        // Optimized path: only outer shell of fully-filled module is exposed
        foreach (Direction dir in Directions)
        {
            for (int u = 0; u < size; u++)
            {
                for (int v = 0; v < size; v++)
                {
                    Vector3I position = GetSurfacePosition(dir, u, v, size);
                    Block block = module.GetBlock(position);

                    if (block.IsEmpty)
                        continue;

                    BlockDefault blockDefault = BlockStore.Instance.GetBlockDefault(block);

                    bool isVoxel = blockDefault is VoxelBlockDefault voxelDefault;
                    bool isModel = blockDefault is ModelBlockDefault;

                    // Add to render cache if voxel
                    if (isVoxel)
                    {
                        var renderInfo = new SurfaceCache<VoxelBlockDefault>.BlockSurfaceInfo(
                            block, (VoxelBlockDefault)blockDefault);
                        renderCache.AddExposedFace(dir, position, renderInfo);
                    }

                    // Add to collision cache if voxel or model
                    if (isVoxel || isModel)
                    {
                        var collisionInfo = new SurfaceCache<BlockDefault>.BlockSurfaceInfo(
                            block, blockDefault);
                        collisionCache.AddExposedFace(dir, position, collisionInfo);
                    }
                }
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