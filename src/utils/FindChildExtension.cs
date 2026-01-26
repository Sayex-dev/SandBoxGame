using Godot;

public static class FindChildExtension
{
    public static T FindChildOfType<T>(this Node parent, int checkDepth) where T : Node
    {
        if (parent == null || checkDepth < 0)
            return null;

        foreach (Node child in parent.GetChildren())
        {
            // Check current child
            if (child is T typedChild)
                return typedChild;

            // Recurse into children if depth allows
            if (checkDepth > 0)
            {
                T result = child.FindChildOfType<T>(checkDepth - 1);
                if (result != null)
                    return result;
            }
        }

        return null;
    }
}