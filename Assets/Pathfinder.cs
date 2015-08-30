using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class Pathfinder
{
    protected bool m_GraphIsDirty = true;

    public void SetGraphDirty()
    {
        m_GraphIsDirty = true;
    }
    abstract public List<Vector2> GetPath(Vector2 location, Vector2 goal, Dictionary<Vector2, TileNode> graph);

}

