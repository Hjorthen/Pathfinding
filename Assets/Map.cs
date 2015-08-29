using System;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Directions
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum PathFindermode
{
    Breadth_First_Search,
    Dijkstras,
    AStar
}

public class Map : MonoBehaviour
{
    public UnityEngine.Object Square;
    public UnityEngine.Object Arrow;
    public PathFindermode Mode;
    public Vector2 Bounds;


    Vector2 MouseStartDragPos;
    Vector2 GoalNode;

    Pathfinder path;
    /// <summary>
    /// Key: Grid position
    /// Value: The Node at that position
    /// </summary>
    private Dictionary<Vector2, TileNode> m_Graph = new Dictionary<Vector2, TileNode>();

    private PathFindermode PrevMode;
    /// <summary>
    /// Key: Grid position
    /// Value: The CTile at that position
    /// </summary>
    private CGrid Grid;


    void Start()
    {
        Grid = new CGrid(Bounds);
        Grid.Arrow = Arrow;
        Grid.Square = Square;

        Grid.Draw();

        //Create nodemap from the grid 
        foreach(var t in Grid.Grid)
        {
            m_Graph[t.Key] = new TileNode(t.Key, t.Value as NodeDebugDisplay);

        }

        
        Vector2[] edge_offsets = {
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(-1, 0),
        new Vector2(1, 0) };

        foreach (var n in m_Graph)
        {
            //Adds the sourrounding tiles as the edges: 
            foreach (Vector2 edge in edge_offsets)
            {
                n.Value.AddEdge(n.Value.Position + edge);
            }
            
        }
        ChangePathfinder();

    }

    void ChangePathfinder()
    {
        switch (Mode)
        {
            case PathFindermode.AStar:
                path = new Astar();
                break;
            case PathFindermode.Breadth_First_Search:
                path = new BreadCrumps();
                break;
            case PathFindermode.Dijkstras:
                path = new Dijkstras();
                break;
        }
        PrevMode = Mode;
    }
    Vector2 WorldToGrid(Vector2 worldPos)
    {
        int tile_x = Mathf.FloorToInt((int)worldPos.x / CGrid.TileSize);
        int tile_y = Mathf.FloorToInt((int)worldPos.y / CGrid.TileSize);
        return new Vector2(tile_x, tile_y);
    }

    //Returns the node at world coordinates
    TileNode WorldToNode(Vector2 pos)
    {
        Vector2 tile = WorldToGrid(pos);
        try
        {

            Debug.Log("Found tile: " + tile + " at position: " + pos);
            return m_Graph[tile];
        }
        catch
        {
            Debug.LogWarning("Failed to locate tile");
            return null;
        }

    }



    

    /// <summary>
    /// Clears a recently drawn path
    /// </summary>

    void Update()
    {
        //Enables switching pathfinding during runtime. 
        if(PrevMode!=Mode)
        {
            ChangePathfinder();
        }
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 tilePos = WorldToGrid(mousePos);

        if (Input.GetMouseButtonDown(0))
        {
            MouseStartDragPos = mousePos;
        }

        float scroll;
        if((scroll = Input.GetAxis("Mouse ScrollWheel"))!=0)
        {
            m_Graph[tilePos].Cost += Mathf.FloorToInt(scroll);
        }

        if(Input.GetMouseButton(2))
        {
            m_Graph[tilePos].Cost = 500;
        }
        if (Input.GetMouseButtonUp(0))
        {
            TileNode n = m_Graph[tilePos];
            if (n != null)
            {

                //Mouse were dragged
                Vector2 draglenght;
                if ((draglenght = (mousePos - MouseStartDragPos)).magnitude > CGrid.TileSize)
                {
                    int tilesMarked = Mathf.CeilToInt(draglenght.magnitude / CGrid.TileSize);
                    Vector2 unitLenght = draglenght / tilesMarked;
                    List<TileNode> markedTiles = new List<TileNode>();
                    for (int i = 0; i <= tilesMarked; i++)
                    {
                        Vector2 markedTilePos = MouseStartDragPos + unitLenght * i;
                        TileNode node = WorldToNode(markedTilePos);
                        if (node != null)
                            markedTiles.Add(node);

                    }

                    foreach (TileNode node in markedTiles)
                    {
                        Grid.Grid[node.Position].SetWall();
                        node.IsWall = true;
                        path.SetGraphDirty();
                    }
                }
                else
                {
                    //Gets the tile which were clicked: 
                    Grid.Grid[n.Position].SetStart();
                    GoalNode = tilePos;
                }
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Grid.Reset();
            List<Vector2> trail = path.GetPath(GoalNode, tilePos, m_Graph);
            foreach(Vector2 p in trail)
            {

                Grid[p].SetPath();
            }


        }
    }

}