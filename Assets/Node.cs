using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Functions to be implented for debugging purposes 
/// </summary>
public interface NodeDebugDisplay
{
    void SetChecked();
    void SetWall();
    void SetPath();
    void SetColor(Color col);
    void SetCostDisplay(string str);
    void SetPathDirection(float angle);
}


public abstract class Node : IEnumerable
{
    public Vector2 Position
    {
        get;
        protected set;
    }

    /// <summary>
    /// Contains the position of the edges
    /// </summary>
    HashSet<Vector2> edges = new HashSet<Vector2>();

    public void AddEdge(Vector2 pos)
    {
        edges.Add(pos);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return edges.GetEnumerator();
    }
}

/// <summary>
/// Contains the position nodes in a weigthed way
/// </summary>
public class WeightedPath : IComparable<WeightedPath>
{
    public WeightedPath(Vector2 pos, int c)
    {
        Position = pos;
        Cost = c;
    }

    int Cost;

    public Vector2 Position;

    public int CompareTo(WeightedPath other)
    {
        if (Cost < other.Cost)
            return -1;
        else if (Cost > other.Cost)
            return 1;
        else
            return 0;
    }
}


public class PolyGridNode : Node, IComparable<PolyGridNode>
{
    Dictionary<Vector2, int> DistanceCosts = new Dictionary<Vector2, int>();

    public int CompareTo(PolyGridNode other)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Contains information used for pathfinding from one tile to another
/// </summary>
/// 
public class TileNode : Node, IComparable<TileNode>
{
    private int m_cost;

    //Used to easily show debugging information
    public NodeDebugDisplay Debug;

    public int Cost
    {
        set
        {
            //Movement cost should never be below 1
            m_cost = value > 0 ? value : 1;

            if (m_cost == int.MaxValue/2)
                Debug.SetCostDisplay("#inf");
            else
                Debug.SetCostDisplay((m_cost.ToString()));
        }
        get
        {
            return m_cost;
        }
    }

   
    /// <summary>
    /// Colors the tile gray and makes it impassable 
    /// </summary>
    protected bool m_isWall;
    public bool IsWall
    {
        set
        {
            if (value == true)
            {
                Debug.SetWall();
                m_isWall = true;
                Cost = int.MaxValue/2;
            }
            else
            {
                Debug.SetColor(Color.yellow);
                m_isWall = false;
                Cost = 1;
            }
        }
        get
        {
            return m_isWall;
        }
    }

    public TileNode(Vector2 pos, NodeDebugDisplay debug)
    {

        Debug = debug;
        Position = pos;
        //SetEdges(pos);
        IsWall = false;
        Cost = UnityEngine.Random.Range(1, 3);
    }
  

    public int CompareTo(TileNode other)
    {
        if (other.Cost > Cost)
            return 1;
        else if (other.Cost < Cost)
            return -1;
        else
            return 0;
    }
}
