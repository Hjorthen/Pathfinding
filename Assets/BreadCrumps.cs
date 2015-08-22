using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;






public class BreadCrumps : Pathfinder{
    
    

    
    private Vector2 MouseStartDragPos;
    

    /// <summary>
    /// Contains a list of nodes and who came before it. 
    /// </summary>
    private Dictionary<Vector2, Vector2> Chain = new Dictionary<Vector2, Vector2>();
    
    


	

    /// <summary>
    /// Performs a breadth first search
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    public override List<Vector2> GetPath(Vector2 location, Vector2 goal, Dictionary<Vector2, Node> graph)
    {
        var path = new List<Vector2>();

        Search(graph, goal, location);

        Vector2 tile = location;
        int i = 0;
        do
        {
            path.Add(tile);
            ++i;
            if (i > 1000)
                break;

        } while (Chain.TryGetValue(tile, out tile));
        return path;
    }

    void Search(Dictionary<Vector2, Node> graph, Vector2 begin, Vector2 target)
    {
        Debug.Log("Search started");
        var frontier = new Queue<Vector2>();
        frontier.Enqueue(begin);

        var visited = new HashSet<Vector2>();
        visited.Add(begin);

        while (frontier.Count > 0)
        {

            var position = frontier.Dequeue();
            Debug.Log("Checking node: " + position);
            Node node = graph[position];
            foreach (Vector2 edge in node)
            {
                if (!visited.Contains(edge) && (edge.x < 16) && (edge.y < 16) && (edge.x > -1) && (edge.y > -1))
                {


                    Node neightbour = graph[edge];
                    if (neightbour.IsWall)
                    {
                        neightbour.Tile.color = Color.gray;
                        continue;
                    }

                    frontier.Enqueue(edge);
                    visited.Add(edge);
                    neightbour.Tile.SetChecked();
                    Chain[edge] = position;

                  
                    float rotation = 0;

                    //Calculates where the neightbour is from the current node;
                    Vector2 dir = node.Position - edge;
                    if (dir.x < 0)
                    {
                        rotation = 90; //The tile is to our right
                    }
                    else if (dir.x > 0)
                    {
                        rotation = 270; //The tile is to our left
                    }
                    else if (dir.y > 0)
                    {
                        rotation = 0; //The tile is above us
                    }
                    else if (dir.y < 0)
                    {
                        rotation = 180; //The tile is below us
                    }

                    neightbour.Tile.PointArrow(rotation);

                    //We found what we were looking for
                    if (position == target)
                        return; 

                }

            }

            
        }
        m_GraphIsDirty = false;
    }


}
