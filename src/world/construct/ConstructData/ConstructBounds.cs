using System;
using System.Collections.Generic;
using Godot;

public class ConstructBounds
{
    public event Action Changed;

    public ConstructGridPos MinPos { get; private set; }
    public ConstructGridPos MaxPos { get; private set; }

    public bool HasAnyBlocks { get; private set; }

    public void Clear()
    {
        HasAnyBlocks = false;
    }

    public void AddPosition(ConstructGridPos pos)
    {
        var oldMin = MinPos;
        var oldMax = MaxPos;

        if (!HasAnyBlocks)
        {
            MinPos = pos;
            MaxPos = pos;
            HasAnyBlocks = true;
            Changed?.Invoke();
            return;
        }

        Vector3I v = pos.Value;

        MinPos = new ConstructGridPos(new Vector3I(
            Math.Min(MinPos.Value.X, v.X),
            Math.Min(MinPos.Value.Y, v.Y),
            Math.Min(MinPos.Value.Z, v.Z)
        ));

        MaxPos = new ConstructGridPos(new Vector3I(
            Math.Max(MaxPos.Value.X, v.X),
            Math.Max(MaxPos.Value.Y, v.Y),
            Math.Max(MaxPos.Value.Z, v.Z)
        ));

        if (MinPos != oldMin || MaxPos != oldMax)
            Changed?.Invoke();
    }

    public void RemovePosition(ConstructGridPos pos, Dictionary<ModuleLocation, Module> modules)
    {
        if (!IsOnBounds(pos))
            return;

        var oldMin = MinPos;
        var oldMax = MaxPos;

        // Rebuild bounds
        Clear();
        foreach (var kvp in modules)
        {
            var moduleLocation = kvp.Key;
            var module = kvp.Value;

            AddPosition(module.MinPos.ToConstruct(moduleLocation, module.ModuleSize));
        }

        if (MinPos != oldMin || MaxPos != oldMax)
            Changed?.Invoke();
    }

    public void CombineWith(ConstructBounds other)
    {
        if (!other.HasAnyBlocks)
            return;

        AddPosition(other.MinPos);
        AddPosition(other.MaxPos);
    }

    public bool IsOnBounds(ConstructGridPos pos)
    {
        return pos.Value.X == MinPos.Value.X || pos.Value.X == MaxPos.Value.X ||
            pos.Value.Y == MinPos.Value.Y || pos.Value.Y == MaxPos.Value.Y ||
            pos.Value.Z == MinPos.Value.Z || pos.Value.Z == MaxPos.Value.Z;
    }
}