using UnityEngine;
using System.Linq;
using System.Collections.Generic;



public class PolyGridGenerator : MonoBehaviour {

    // Use this for initialization
    Dictionary<Vector2, TileNode> PolyGrid = new Dictionary<Vector2, TileNode>();


    void OnDrawGizmos()
    {
        PolyGrid.Clear();

        //Gameobjects representing the two positions we are pathfidning between
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        Vector2 goalPos = target.transform.position;GameObject.FindGameObjectWithTag("Player");

        GameObject monster = GameObject.FindGameObjectWithTag("Finish");
        Vector2 monsterPos = monster.transform.position;

        //Workaround for the rays hitting the unit when exploring vertexes, which results in no valid paths
        int targetLayer = target.layer;
        target.layer = 2;

        int monsterLayer = monster.layer;
        monster.layer = 2;



        Gizmos.color = Color.magenta;
        BoxCollider2D[] colliders = GameObject.FindObjectsOfType<BoxCollider2D>();
        Vector2[] corners = { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1) };
        //List<Vector2> Vertices = new List<Vector2>();

        foreach (BoxCollider2D collider in colliders)
        {

            //Objects with rigidbodies are skipped because they WILL move a lot
            if (collider.attachedRigidbody != null)
                continue;

            //Colors the colliders to visualize which ones it sees
            collider.GetComponent<SpriteRenderer>().color = Color.cyan;

            //BOUNDS IS A AABB CHECK! Which means it can't be used to figure out the points.. 
            Bounds area = collider.bounds;
            Gizmos.DrawSphere(area.center, 5);
            
            //Finds each corner of each and every boxcollider
            foreach(Vector2 cornerOffset in corners)
            {
                //The + 0.1f is to avoid the raycast from colliding with the collision box which we are trying to avoid
                Vector2 cornerPos = new Vector2(collider.offset.x + cornerOffset.x * (collider.size.x/ 2 + 0.1f), collider.offset.y + cornerOffset.y * (collider.size.y/2 + 0.1f));
                Vector2 worldPos = collider.transform.TransformPoint(cornerPos);
                //Adds each corner to a list of vertices which can be used to generate the map
                PolyGrid.Add(worldPos, new TileNode(worldPos, new LineDebugger()));
                Gizmos.DrawSphere(worldPos, 5);

            }


        }
        Debug.Log("Found " + colliders.Length + " colliders");

        //Loops over each vertex and raycasts to the other vertices to check if they have LOS
        int rayCounter = 0;
        //Reuseable ray
        RaycastHit2D[] ray = new RaycastHit2D[1];

        //For testing purposes. Adds an end and start location: 
        PolyGrid.Add(monsterPos, new TileNode(monsterPos, new LineDebugger()));
        PolyGrid.Add(goalPos, new TileNode(goalPos, new LineDebugger()));

        foreach (Vector2 vertex in PolyGrid.Keys)
        {
            foreach (Vector2 otherVertex in PolyGrid.Keys)
            {
                //We dont want to check LOS for the current vertex
                if (otherVertex == vertex)
                    continue;
                
                //If we didn't hit any objects between the two vertices then we can add it as a valid path
                if (Physics2D.LinecastNonAlloc(vertex, otherVertex, ray) == 0)
                {
                    //RAINBOWS
#if false
                    Color[] Colors = {  Color.blue, Color.magenta, Color.green, Color.yellow, Color.red };
                    Gizmos.color = Colors[Random.Range(0, 5)];
#endif
                  
                    Gizmos.DrawLine(vertex, otherVertex);

                    //Adds the node as a possible vertex
                    PolyGrid[vertex].AddEdge(otherVertex);
                    ++rayCounter;
                }
            }

        }
        Debug.Log("Casted " + rayCounter + " rays on " + PolyGrid.Count + " vertices");
      
        Astar path = new AStarPolyGrid();

        Vector2 prev = goalPos;
        List<Vector2> route = path.GetPath(monsterPos, goalPos, PolyGrid);
        Debug.Log("Route contained: " + route.Count);
        foreach(Vector2 waypoint in route)
        {

            Gizmos.color = Color.green;
            Gizmos.DrawLine(prev, waypoint);
            prev = waypoint;
        }
        target.layer = targetLayer;
        monster.layer = monsterLayer; 
    }

        // Update is called once per frame 

}
