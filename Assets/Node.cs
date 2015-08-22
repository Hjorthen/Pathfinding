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

/// <summary>
/// Contains information used for pathfinding from one tile to another
/// </summary>
public class Node : IEnumerable, IComparable<Node>
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

    static Vector2[] edge_offsets = {
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(-1, 0),
        new Vector2(1, 0) };
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

    public Vector2 Position
    {
        get;
        private set;
    }

    /// <summary>
    /// Contains the position of the edges
    /// </summary>
    Vector2[] edges = new Vector2[4];

    public Node(Vector2 pos, NodeDebugDisplay debug)
    {

        Debug = debug;
        Position = pos;
        SetEdges(pos);
        IsWall = false;
        Cost = UnityEngine.Random.Range(1, 3);
    }
    public void SetEdges(Vector2 pos)
    {
        for (int i = 0; edges.Length > i; ++i)
        {
            edges[i] = pos + edge_offsets[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return edges.GetEnumerator();
    }

    public int CompareTo(Node other)
    {
        if (other.Cost > Cost)
            return 1;
        else if (other.Cost < Cost)
            return -1;
        else
            return 0;
    }
}
