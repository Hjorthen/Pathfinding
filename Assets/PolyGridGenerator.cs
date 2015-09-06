using UnityEngine;
using System.Linq;
using System.Collections.Generic;




public class PolyGridGenerator : MonoBehaviour {

    // Use this for initialization
    Dictionary<Vector2, TileNode> PolyGrid = new Dictionary<Vector2, TileNode>();
    public bool DebugDraw = false;
    void Start()
    {
        GenerateGrid();
    }
    
    public void GenerateGrid()
    {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            PolyGrid.Clear();

            //Gameobjects representing the two positions we are pathfidning between
            GameObject target = GameObject.FindGameObjectWithTag("Player");
            Vector2 goalPos = target.transform.position; GameObject.FindGameObjectWithTag("Player");

            GameObject monster = GameObject.FindGameObjectWithTag("Finish");
            Vector2 monsterPos = monster.transform.position;

            //Workaround for the rays hitting the unit itself when exploring vertexes, which results in no valid paths
            int targetLayer = target.layer;
            target.layer = 2;

            int monsterLayer = monster.layer;
            monster.layer = 2;



            Collider2D[] colliders = GameObject.FindObjectsOfType<Collider2D>();
            Vector2[] corners = { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1) };
            //List<Vector2> Vertices = new List<Vector2>();

            foreach (Collider2D collider in colliders)
            {

                //Objects with rigidbodies are skipped because they WILL move a lot. We also skip disabled objects as units shouldn't try to avoid these. 
                if (collider.attachedRigidbody != null || !collider.isActiveAndEnabled)
                    continue;

                //Colors the colliders to visualize which ones it sees
#if DEBUG
                collider.GetComponent<SpriteRenderer>().color = Color.cyan;
#endif
               
                //Finds each corner of each and every boxcollider

                BoxCollider2D boxCollider = collider as BoxCollider2D;
                CircleCollider2D circleCollider;
                PolygonCollider2D polyCollider;

                if ((boxCollider = collider as BoxCollider2D) != null)
                {
                    foreach (Vector2 cornerOffset in corners)
                    {
                        //The + 0.1f is to avoid the raycast from colliding with the collision box which we are trying to avoid
                        Vector2 cornerPos = new Vector2(collider.offset.x + cornerOffset.x * (boxCollider.size.x / 2 + 0.1f), collider.offset.y + cornerOffset.y * (boxCollider.size.y / 2 + 0.1f));
                        Vector2 worldPos = collider.transform.TransformPoint(cornerPos);
                        //Adds each corner to a list of vertices which can be used to generate the map
                        PolyGrid.Add(worldPos, new TileNode(worldPos, new LineDebugger()));
                    }
                    continue;
                }
                else if ((circleCollider = collider as CircleCollider2D) != null)
                {

                    foreach (Vector2 offset in corners)
                    {

                        Vector2 offsetPos = circleCollider.offset + (circleCollider.radius + 1) * offset;
                        Vector2 worldPos = collider.transform.TransformPoint(offsetPos);
                        PolyGrid.Add(worldPos, new TileNode(worldPos, new LineDebugger()));
                    }
                    continue;
                }
                else if ((polyCollider = collider as PolygonCollider2D) != null)
                {
                    //Loops over each vertex in the polygon collider and transform them to world space. 
                    foreach (Vector2 point in polyCollider.points)
                    {
                        //By how much should we multiply the vector with to incrase its lenght by 1?
                        float multiplyLenght = (point.magnitude + 1) / point.magnitude;
                        Vector2 worldPos = collider.transform.TransformPoint(point * multiplyLenght);
                        PolyGrid.Add(worldPos, new TileNode(worldPos, new LineDebugger()));
                        
                    }
                }


            }
            Debug.Log("Found " + colliders.Length + " colliders");

            //Loops over each vertex and raycasts to the other vertices to check if they have LOS
            int rayCounter = 0;

            //Reuseable ray which saves memory allocations.
            RaycastHit2D[] ray = new RaycastHit2D[1];

            //For testing purposes. Adds an end and start location: 
            PolyGrid.Add(monsterPos, new TileNode(monsterPos, new LineDebugger()));
            PolyGrid.Add(goalPos, new TileNode(goalPos, new LineDebugger()));

            //Checks for LOS from every node to every other node. 
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
            foreach (Vector2 waypoint in route)
            {
                prev = waypoint;
            }

            //Resets the layer back to the prior set before the raycast hackfix
            target.layer = targetLayer;
            monster.layer = monsterLayer;
            watch.Stop();
            Debug.Log("Generated Polygrid in " + watch.ElapsedMilliseconds + " miliseconds");
        }
    
        void OnDrawGizmos()
        {
            if (!DebugDraw)
                return;
            Gizmos.color = Color.magenta;
            
            //Contains a list of the nodes already visited
            HashSet<Vector2> visited = new HashSet<Vector2>();
            foreach(KeyValuePair<Vector2, TileNode> Value in PolyGrid)
            {
                Gizmos.DrawSphere(Value.Key, 5);
                visited.Add(Value.Key);
                foreach(Vector2 neighbour in Value.Value)
                {
                    //if true, then we have already visited this node and drawn lines from it to its neighbours. There is therefore no reason to draw it again. 
                    if(!visited.Contains(neighbour))
                    {
                      Gizmos.DrawLine(Value.Key, neighbour);
                    }
                }
            }
        }
   


}
