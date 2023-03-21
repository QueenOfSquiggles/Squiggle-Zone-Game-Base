namespace queen.items;

using System.Collections.Generic;
using Godot;

public class ItemContainerComparer : IComparer<Vector2I>
{

    public Vector2I Size { get; set; }

    public ItemContainerComparer(Vector2I m_size)
    {
        Size = m_size;
    }

    public int Compare(Vector2I x, Vector2I y)
    {
        // use grid sort for organization to guarantee pulling is logical and not Euclidean Distance based.
        int indexX = x.X + (x.Y * Size.X);
        int indexY = y.X + (y.Y * Size.X);
        return indexX < indexY ? 1 : (indexX > indexY ? -1 : 0);
    }
}