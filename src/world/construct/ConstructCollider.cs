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
        SetupCollider(construct);
    }

    public async void SetupCollider(Construct construct)
    {
        directionalCollisionBlocks.Clear();
        Dictionary<Vector3I, Module> modules = construct.GetModules();
        foreach (KeyValuePair<Vector3I, Module> kvp in modules)
        {
            Module module = kvp.Value;
            Vector3I inConstructModulePos = kvp.Key;
            int[] blocks = module.GetBlockArray();

            for (int i = 0; i < blocks.Length; i++)
            {
                int blockId = blocks[i];
                if (blockId == -1) continue;
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {

                }
            }
        }
    }

    public void AddBlockCollider(Construct construct, Vector3I inConstructPos)
    {
        foreach (Vector3I inConstructPos in construct.)
        {

        }
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {

        }
    }

    public void RemoveBlockCollider(Construct construct, Vector3I inConstructPos)
    {

    }
}