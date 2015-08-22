using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Binary_Heap;
using System;

/// <summary>
/// Contains 
/// </summary>
class WeightedPath : IComparable<WeightedPath>
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

class Dijkstras : Pathfinder
{
    
    private int CalculateCost(Node a, Node b)
    {
        return b.Cost;
    }

    public override List<Vector2> GetPath(Vector2 location, Vector2 goal, Dictionary<Vector2, Node> graph)
    {
        //Using a binaryMinHeap as a priority queue 
        var frontier = new BinaryMinHeap<WeightedPath>();
        

        Dictionary<Vector2, int> cost_so_far = new Dictionary<Vector2, int>();
        Dictionary<Vector2, Vector2> chain = new Dictionary<Vector2, Vector2>();

        //Sets 0 priority to make sure its called first
        frontier.Insert(new WeightedPath(location, 0));
        cost_so_far.Add(location, 0);

        Vector2 current = location;
        while (frontier.GetNodeCount()!=0)
        {
          
            current = frontier.Extract().Position;
            if (current == goal) 
                 break;

            //Checking the 4 sourrounding tiles from the current one
            foreach(Vector2 edge in graph[current])
            {
                //Prints the position of the current tile
                Debug.Log(edge.ToString());

                //Checks if the tile is within bounds(TODO: mark each sourrounding tile as a wall)
                if (!((edge.x < 16) && (edge.y < 16) && (edge.x > -1) && (edge.y > -1)) || graph[edge].IsWall)
                {
                    continue;
                }

                if (edge != location) 
                    graph[current].Debug.SetColor(Color.magenta);

                //Calculates what the cost will be to move from the current tile to the next
                int new_cost = cost_so_far[current] + CalculateCost(graph[current], graph[edge]);

                //Checks to see if we have already checked this next tile and if the current path is "cheaper"
                if((!cost_so_far.ContainsKey(edge) || new_cost < cost_so_far[edge]))
                {
                    graph[edge].Debug.SetChecked();
                    cost_so_far[edge] = new_cost;
                    frontier.Insert(new WeightedPath(edge, new_cost));
                    chain[edge] = current;
                }   
            }
        }

        //Reverses the path from the end back to the start and returns it
        var path = new List<Vector2>();
        Vector2 tile = goal;
        int i = 0;
        do
        {
            path.Add(tile);
            ++i;
            if (i > 1000)
                break;

        } while (chain.TryGetValue(tile, out tile));
        return path;

    }
}

