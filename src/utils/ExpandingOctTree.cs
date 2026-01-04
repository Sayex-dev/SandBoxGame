using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IHaveBoundingBox
{
    public Vector3I GetRootPos();
    public Vector3I GetMin();
    public Vector3I GetMax();
}

public class ExpandingOctTree<T> where T : IHaveBoundingBox
{
    private class Node
    {
        public HashSet<T> BoundObjects = new HashSet<T>();
        public Node[] Children;
        public bool IsEmpty
        {
            get
            {
                if (BoundObjects.Count > 0)
                    return false;

                if (Children != null)
                    return false;

                return true;
            }
        }
        public bool HasChildren
        {
            get
            {
                return Children != null;
            }
        }

        public void Populate()
        {
            if (Children == null)
            {
                Children = new Node[8];
                for (int i = 0; i < 8; i++)
                {
                    Children[i] = new Node();
                }
            }
        }

        public void Collapse()
        {
            Children = null;
        }
    }

    private Node root;
    private int rootSize;
    private int minNodeSize;
    private Vector3I rootOrigin;
    private HashSet<T> globalObjects = new HashSet<T>();
    public ExpandingOctTree(int minSize, Vector3I treeOrigin)
    {
        rootSize = minSize;
        minNodeSize = minSize;
        rootOrigin = treeOrigin;
        root = new Node();
    }

    public void Insert(T t)
    {
        EnsureContains(t);
        RecursiveInsert(root, rootOrigin, rootSize, t);
    }

    public void InsertGlobal(T t)
    {
        globalObjects.Add(t);
    }

    public List<T> QueryBox(Vector3I min, Vector3I max)
    {
        List<T> results = globalObjects.ToList();
        QueryInternal(root, rootOrigin, rootSize, min, max, results);
        return results;
    }

    public List<T> QueryAt(Vector3I pos)
    {
        return QueryBox(pos, pos);
    }

    public void Remove(T t)
    {
        if (globalObjects.Contains(t)) globalObjects.Remove(t);
        RemoveInternal(root, rootOrigin, rootSize, t);
    }

    private void RemoveInternal(Node node, Vector3I origin, int size, T t)
    {
        if (node.IsEmpty) return;

        if (node.BoundObjects.Contains(t))
        {
            node.BoundObjects.Remove(t);
            Collapse(node);
            return;
        }

        if (node.HasChildren)
        {
            for (int i = 0; i < 8; i++)
            {
                int childSize = size / 2;
                Vector3I childOrigin = GetChildOrigin(origin, childSize, i);
                if (FitsInside(childOrigin, childSize, t))
                {
                    Node child = node.Children[i];
                    RemoveInternal(child, childOrigin, childSize, t);
                    return;
                }
            }
        }
    }

    private void Collapse(Node node)
    {
        foreach (Node child in node.Children)
        {
            if (!child.IsEmpty)
            {
                return;
            }
        }

        node.Collapse();
    }

    private void QueryInternal(Node node, Vector3I origin, int size, Vector3I min, Vector3I max, List<T> outList)
    {
        if (node.IsEmpty) return;

        if (!BoxesOverlap(origin, origin + new Vector3I(size, size, size), min, max))
            return;

        foreach (T t in node.BoundObjects)
        {
            if (BoxesOverlap(t.GetMin(), t.GetMax(), min, max))
                outList.Add(t);
        }

        if (!node.HasChildren) return;

        int half = size / 2;
        for (int i = 0; i < 8; i++)
        {
            Node child = node.Children[i];
            if (child.IsEmpty) continue;
            Vector3I childOrigin = GetChildOrigin(origin, half, i);
            QueryInternal(child, childOrigin, half, min, max, outList);
        }
    }

    private bool BoxesOverlap(Vector3I amin, Vector3I amax, Vector3I bmin, Vector3I bmax)
    {
        return !(amax.X < bmin.X || amin.X > bmax.X ||
                 amax.Y < bmin.Y || amin.Y > bmax.Y ||
                 amax.Z < bmin.Z || amin.Z > bmax.Z);
    }

    private void RecursiveInsert(Node node, Vector3I origin, int size, T t)
    {
        if (size > minNodeSize)
        {
            for (int i = 0; i < 8; i++)
            {
                int childSize = size / 2;
                Vector3I childOrigin = GetChildOrigin(origin, childSize, i);
                if (FitsInside(childOrigin, childSize, t))
                {
                    if (node.HasChildren) node.Populate();
                    Node child = node.Children[i];
                    RecursiveInsert(child, childOrigin, childSize, t);
                    return;
                }
            }
        }

        // If the code reaches this point no child was able to contain the whole object. 
        // Meaning it must be placed withing the current child.

        node.BoundObjects.Add(t);
    }

    private void EnsureContains(T t)
    {
        // Expansion
        while (!FitsInside(rootOrigin, rootSize, t))
        {
            ExpandRoot(t);
        }
    }

    private bool FitsInside(Vector3I origin, int size, T t)
    {
        Vector3I min = t.GetMin();
        Vector3I max = t.GetMax();
        return
            origin.X <= min.X &&
            origin.Y <= min.Y &&
            origin.Z <= min.Z &&
            origin.X + size < max.X &&
            origin.Y + size < max.Y &&
            origin.Z + size < max.Z;

    }

    private Vector3I GetChildOrigin(Vector3I origin, int half, int index)
    {
        Vector3I o = origin;

        if ((index & 1) != 0) o.X += half;
        if ((index & 2) != 0) o.Y += half;
        if ((index & 4) != 0) o.Z += half;

        return o;
    }

    private void ExpandRoot(T t)
    {
        int newSize = rootSize * 2;
        Vector3I newOrigin = rootOrigin;

        Vector3I min = t.GetMin();
        if (min.X < rootOrigin.X) newOrigin.X -= rootSize;
        if (min.Y < rootOrigin.Y) newOrigin.Y -= rootSize;
        if (min.Z < rootOrigin.Z) newOrigin.Z -= rootSize;

        Node newRoot = new Node();
        newRoot.Children = new Node[8];

        int idx = 0;
        if (rootOrigin.X >= newOrigin.X + rootSize) idx |= 1;
        if (rootOrigin.Y >= newOrigin.Y + rootSize) idx |= 2;
        if (rootOrigin.Z >= newOrigin.Z + rootSize) idx |= 4;

        newRoot.Children[idx] = root;

        root = newRoot;
        rootOrigin = newOrigin;
        rootSize = newSize;
    }
}