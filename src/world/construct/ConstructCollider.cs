using Godot;
using System;
using System.Collections.Generic;

public class ConstructCollision
{
    public ConstructCollider OtherConstruct;
    public Vector3I BlockPosOnOther;
    public Vector3I BlockPosOnThis;
}

public class ConstructCollider
{
    private Dictionary<Direction, Vector3I[]> directionalCollisionBlocks = new();

    public ConstructCollider(Construct construct)
    {
    }

    public async void SetupCollider(Construct construct)
    {
        directionalCollisionBlocks.Clear();
        Dictionary<ModuleLocation, Module> modules = construct.GetModules();
        foreach (KeyValuePair<ModuleLocation, Module> kvp in modules)
        {
            Module module = kvp.Value;
            ModuleLocation moduleLocation = kvp.Key;

            int[] blocks = module.GetBlockArray();

            for (int i = 0; i < blocks.Length; i++)
            {
                int blockId = blocks[i];
                if (blockId == -1) continue;
                ModuleGridPos modulePos = module.ArrayToInModulePos(i);
                ConstructGridPos constructPos = modulePos.ToConstruct(moduleLocation, construct.ModuleSize);
                Dictionary<Direction, bool> collideableDirections = GetCollideableDirections(construct, constructPos);
            }
        }
    }

    public void AddBlockCollider(Construct construct, ConstructGridPos constructGridPos)
    {
    }

    public void RemoveBlockFromCollider(Construct construct, Vector3I worldPos)
    {
    }

    private Dictionary<Direction, bool> GetCollideableDirections(Construct construct, ConstructGridPos constructPos)
    {
        Dictionary<Direction, bool> collideableDirections = [];
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            WorldGridPos checkPos = new(constructPos.Value + (Vector3I)DirectionTools.GetWorldDirVec(dir));
            int checkBlockId;
            if (!construct.HasBlock(checkPos, out checkBlockId)) continue;
            collideableDirections[dir] = checkBlockId != -1;
        }
        return collideableDirections;
    }
}