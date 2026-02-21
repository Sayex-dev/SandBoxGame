using System.Collections.Generic;

public interface IWorldQuery
{
    /// <summary>
    /// Returns true if any block exists at the given world position (from any construct).
    /// </summary>
    bool HasBlockAt(WorldGridPos worldPos);

    /// <summary>
    /// Returns all constructs whose bounds overlap the given box.
    /// </summary>
    List<Construct> GetConstructsInArea(WorldGridPos min, WorldGridPos max);
}
