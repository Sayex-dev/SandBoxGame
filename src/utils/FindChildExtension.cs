using System.Collections.Generic;
using Godot;

public static class FindChildExtension
{
    public static T FindChildOfType<T>(this Node parent) where T : Node
    {
        foreach (Node child in parent.GetChildren())
        {
            if (child is T typedChild)
                return typedChild;
        }

        return null;
    }

    public static List<T> FindChildrenOfType<T>(this Node parent) where T : Node
    {
        List<T> children = [];
        foreach (Node child in parent.GetChildren())
        {
            if (child is T typedChild)
                children.Add(typedChild);
        }
        return children;
    }
}