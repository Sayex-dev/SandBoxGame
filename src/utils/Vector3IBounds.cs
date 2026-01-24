using Godot;

public class Vector3IBounds
{
    private int size;
    private int[] xPlaneCounts;
    private int[] yPlaneCounts;
    private int[] zPlaneCounts;

    public Vector3I MaxPos { get; private set; }
    public Vector3I MinPos { get; private set; }

    public Vector3IBounds(int size)
    {
        this.size = size;

        // Initialize plane counters
        xPlaneCounts = new int[size];
        yPlaneCounts = new int[size];
        zPlaneCounts = new int[size];

        // Initialize with invalid bounds
        MinPos = new Vector3I(int.MaxValue, int.MaxValue, int.MaxValue);
        MaxPos = new Vector3I(int.MinValue, int.MinValue, int.MinValue);
    }

    public void AddPoint(Vector3I point, int totalPointCount)
    {
        UpdatePlaneCountsOnAdd(point);
        UpdateBoundsAfterAdd(point, totalPointCount);
    }

    public void RemovePoint(Vector3I point, int totalPointCount)
    {
        UpdatePlaneCountsOnRemove(point);
        UpdateBoundsAfterRemove(point, totalPointCount);
    }

    public bool IsValidPoint(Vector3I point)
    {
        return point.X >= 0 && point.X < size &&
               point.Y >= 0 && point.Y < size &&
               point.Z >= 0 && point.Z < size;
    }

    public bool HasValidBounds => MinPos.X != int.MaxValue;

    private void UpdatePlaneCountsOnAdd(Vector3I point)
    {
        xPlaneCounts[point.X]++;
        yPlaneCounts[point.Y]++;
        zPlaneCounts[point.Z]++;
    }

    private void UpdatePlaneCountsOnRemove(Vector3I point)
    {
        xPlaneCounts[point.X]--;
        yPlaneCounts[point.Y]--;
        zPlaneCounts[point.Z]--;
    }

    private void UpdateBoundsAfterAdd(Vector3I point, int totalPointCount)
    {
        if (totalPointCount == 1) // First point
        {
            MinPos = point;
            MaxPos = point;
        }
        else
        {
            // Simply expand bounds - O(1)
            Vector3I minVal = new Vector3I(
                Mathf.Min(MinPos.X, point.X),
                Mathf.Min(MinPos.Y, point.Y),
                Mathf.Min(MinPos.Z, point.Z)
            );

            Vector3I maxVal = new Vector3I(
                Mathf.Max(MaxPos.X, point.X),
                Mathf.Max(MaxPos.Y, point.Y),
                Mathf.Max(MaxPos.Z, point.Z)
            );

            MinPos = minVal;
            MaxPos = maxVal;
        }
    }

    private void UpdateBoundsAfterRemove(Vector3I point, int totalPointCount)
    {
        if (totalPointCount == 0)
        {
            // No points left, reset bounds
            MinPos = new Vector3I(int.MaxValue, int.MaxValue, int.MaxValue);
            MaxPos = new Vector3I(int.MinValue, int.MinValue, int.MinValue);
            return;
        }

        // Check if we need to shrink bounds
        Vector3I newMin = MinPos;
        Vector3I newMax = MaxPos;

        // Update X bounds
        if (point.X == MinPos.X && xPlaneCounts[point.X] == 0)
            newMin.X = FindNextNonEmptyPlane(xPlaneCounts, MinPos.X, true);
        if (point.X == MaxPos.X && xPlaneCounts[point.X] == 0)
            newMax.X = FindNextNonEmptyPlane(xPlaneCounts, MaxPos.X, false);

        // Update Y bounds
        if (point.Y == MinPos.Y && yPlaneCounts[point.Y] == 0)
            newMin.Y = FindNextNonEmptyPlane(yPlaneCounts, MinPos.Y, true);
        if (point.Y == MaxPos.Y && yPlaneCounts[point.Y] == 0)
            newMax.Y = FindNextNonEmptyPlane(yPlaneCounts, MaxPos.Y, false);

        // Update Z bounds
        if (point.Z == MinPos.Z && zPlaneCounts[point.Z] == 0)
            newMin.Z = FindNextNonEmptyPlane(zPlaneCounts, MinPos.Z, true);
        if (point.Z == MaxPos.Z && zPlaneCounts[point.Z] == 0)
            newMax.Z = FindNextNonEmptyPlane(zPlaneCounts, MaxPos.Z, false);

        MinPos = newMin;
        MaxPos = newMax;
    }

    private int FindNextNonEmptyPlane(int[] planeCounts, int startIndex, bool searchForward)
    {
        if (searchForward)
        {
            for (int i = startIndex + 1; i < planeCounts.Length; i++)
            {
                if (planeCounts[i] > 0)
                    return i;
            }
        }
        else
        {
            for (int i = startIndex - 1; i >= 0; i--)
            {
                if (planeCounts[i] > 0)
                    return i;
            }
        }
        return startIndex; // Fallback (shouldn't happen if totalPointCount > 0)
    }

    public Vector3I GetBoundsSize()
    {
        if (!HasValidBounds)
            return Vector3I.Zero;

        return MaxPos - MinPos + Vector3I.One;
    }

    public int GetBoundsVolume()
    {
        Vector3I size = GetBoundsSize();
        return size.X * size.Y * size.Z;
    }

    public bool IsPointOnBoundary(Vector3I point)
    {
        if (!HasValidBounds)
            return false;

        return point.X == MinPos.X || point.X == MaxPos.X ||
               point.Y == MinPos.Y || point.Y == MaxPos.Y ||
               point.Z == MinPos.Z || point.Z == MaxPos.Z;
    }
}