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


public class Map : MonoBehaviour
{
    public UnityEngine.Object Square;
    public UnityEngine.Object Arrow;

    public Vector2 Bounds;


    Vector2 MouseStartDragPos;
    Vector2 GoalNode;

    Pathfinder path;
    /// <summary>
    /// Key: Grid position
    /// Value: The Node at that position
    /// </summary>
    private Dictionary<Vector2, Node> m_Graph = new Dictionary<Vector2, Node>();

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
            m_Graph[t.Key] = new Node(t.Key, t.Value);
        }

        path = new Dijkstras();
    }

    Vector2 WorldToGrid(Vector2 worldPos)
    {
        int tile_x = Mathf.FloorToInt((int)worldPos.x / CGrid.TileSize);
        int tile_y = Mathf.FloorToInt((int)worldPos.y / CGrid.TileSize);
        return new Vector2(tile_x, tile_y);
    }

    //Returns the node at world coordinates
    Node WorldToNode(Vector2 pos)
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
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 tilePos = WorldToGrid(mousePos);

        if (Input.GetMouseButtonDown(0))
        {
            MouseStartDragPos = mousePos;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Node n = m_Graph[tilePos];
            if (n != null)
            {

                //Mouse were dragged
                Vector2 draglenght;
                if ((draglenght = (mousePos - MouseStartDragPos)).magnitude > CGrid.TileSize)
                {
                    int tilesMarked = Mathf.CeilToInt(draglenght.magnitude / CGrid.TileSize);
                    Vector2 unitLenght = draglenght / tilesMarked;
                    List<Node> markedTiles = new List<Node>();
                    for (int i = 0; i <= tilesMarked; i++)
                    {
                        Vector2 markedTilePos = MouseStartDragPos + unitLenght * i;
                        Node node = WorldToNode(markedTilePos);
                        if (node != null)
                            markedTiles.Add(node);

                    }

                    foreach (Node node in markedTiles)
                    {
                        Grid.Grid[node.Position].color = Color.gray;
                        node.IsWall = true;
                        path.SetGraphDirty();
                    }
                }
                else
                {
                    //Gets the tile which were clicked: 
                   Grid.Grid[n.Position].color = Color.cyan;
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
                if(p!=GoalNode)
                    Grid[p].color = Color.blue;
            }


        }
    }

}