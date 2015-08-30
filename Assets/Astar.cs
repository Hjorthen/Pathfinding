using System;
using System.Collections.Generic;
using Binary_Heap;
using UnityEngine;


public class AStarSquareGrid : Astar
{
    protected override float CalculateCost(TileNode a, TileNode b)
    {
        return b.Cost;
    }

    protected override float heuristic(Vector2 a, Vector2 b)
    {
        return (a.x - b.x) + (a.y - b.y);
    }
}

public class AStarPolyGrid : Astar
{
    protected override float heuristic(Vector2 a, Vector2 b)
    {
            return (a - b).magnitude;
    }

    protected override float CalculateCost(TileNode a, TileNode b)
    {
            return (a.Position - b.Position).magnitude;
    }
}

   public abstract class Astar : Pathfinder
    {
        protected abstract float heuristic(Vector2 a, Vector2 b);

        protected abstract float CalculateCost(TileNode a, TileNode b);
        

    public override List<Vector2> GetPath(Vector2 location, Vector2 goal, Dictionary<Vector2, TileNode> graph)
        {
            BinaryMinHeap<WeightedPath> frontier = new BinaryMinHeap<WeightedPath>();
            Dictionary<Vector2, Vector2> chained_path = new Dictionary<Vector2, Vector2>();
            Dictionary<Vector2, float> cost_so_far = new Dictionary<Vector2, float>();

            frontier.Insert(new WeightedPath(location, 0));
            cost_so_far.Add(location, 0);

            while(frontier.GetNodeCount() > 0)
            {
                //Gets the lowest cost node from the frontier 
                Vector2 currentNode = frontier.Extract().Position;

                //If we have reached our goal there is no need to search further
                if (currentNode == goal)
                    break;

                foreach(Vector2 adjecentNode in graph[currentNode])
                {
                /*
                    if (!((adjecentNode.x < 16) && (adjecentNode.y < 16) && (adjecentNode.x > -1) && (adjecentNode.y > -1)) || graph[adjecentNode].IsWall)
                    {
                        continue;
                    }
                */
                        float newCost = cost_so_far[currentNode] + CalculateCost(graph[currentNode], graph[adjecentNode]);
                        if((!cost_so_far.ContainsKey(adjecentNode) || newCost < cost_so_far[adjecentNode]))
                        {
                            graph[adjecentNode].Debug.SetChecked();
                            cost_so_far[adjecentNode] = newCost;
                            float priority = newCost + heuristic(goal, adjecentNode);
                            frontier.Insert(new WeightedPath(adjecentNode, priority));
                            chained_path[adjecentNode] = currentNode;
                        }
                }
                

            }
        var path = new List<Vector2>();
        Vector2 tile = goal;
        int i = 0;
        do
        {
            path.Add(tile);
            ++i;
            if (i > 1000)
                break;

        } while (chained_path.TryGetValue(tile, out tile));
        return path;

    }


   }
  
