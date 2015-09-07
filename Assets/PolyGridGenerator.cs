using UnityEngine;
using System.Linq;
using System.Collections.Generic;

struct Vector2Pair
{
    public Vector2Pair(Vector2 one, Vector2 two)
    {
        First = one;
        Second = two;
    }
    public Vector2 First;
    public Vector2 Second;

}


public class PolyGridGenerator : MonoBehaviour {

    // Use this for initialization
    Dictionary<Vector2, TileNode> PolyGrid = new Dictionary<Vector2, TileNode>();

    //How close together should the two vertexes be to get merged? 
    //Setting to 0 disables the feature. 
    public float MergeDistance = 0f;

    [Tooltip("Enables visual drawing of the paths and nodes")]
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

            //Used to locate each corner by using the offset of each collider 
            Vector2[] corners = { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1) };
         

            foreach (Collider2D collider in colliders)
            {

                //Objects with rigidbodies are skipped because they WILL move a lot. We also skip disabled objects as units shouldn't try to avoid these. 
                if (collider.attachedRigidbody != null || !collider.isActiveAndEnabled)
                    continue;

                //Colors the colliders to visualize which ones it sees
#if DEBUG
                collider.GetComponent<SpriteRenderer>().color = Color.cyan;
#endif
               
               

                BoxCollider2D boxCollider = collider as BoxCollider2D;
                CircleCollider2D circleCollider;
                PolygonCollider2D polyCollider;

                //Finds each corner of each and every boxcollider
                if ((boxCollider = collider as BoxCollider2D) != null)
                {
                    foreach (Vector2 cornerOffset in corners)
                    {
                        //The + 0.1f is to avoid the raycast from colliding with the collision box containing the nodes
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
                        //We don't need the exact distance, so we can avoid the expensive square root!
                        float distanceFromCenter = Vector2.SqrMagnitude(point);

                        //By how much should we multiply the vector with to incrase its lenght by 1?
                        float multiplyLenght = (distanceFromCenter + 1) / distanceFromCenter;
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

            List<Vector2Pair> nodesToMerge = new List<Vector2Pair>();

            //Checks for LOS from every node to every other node. 
            foreach (Vector2 vertex in PolyGrid.Keys)
            {
                foreach (Vector2 otherVertex in PolyGrid.Keys)
                {
                    //We dont want to check LOS for the current vertex also checks if we already have checked LOS but from the other direction
                    //In that case the current vertex will already contain the other vertex as an edge
                    if (otherVertex == vertex && PolyGrid[vertex].HasEdge(otherVertex))
                        continue;

                    //If we didn't hit any objects between the two vertices then we can add it as a valid path
                    if (Physics2D.LinecastNonAlloc(vertex, otherVertex, ray) == 0)
                    {
                    
                        if(ray[0].distance < MergeDistance)
                        {
                          nodesToMerge.Add(new Vector2Pair(vertex, otherVertex));
                        }
                        //Adds the node as a possible vertex
                        PolyGrid[vertex].AddEdge(otherVertex);
                        //Adds the current vertex to the other vertex's edge list to avoid raycasting in the opposite direction. 
                        PolyGrid[otherVertex].AddEdge(vertex);    

                        ++rayCounter;
                    }
                }

            }

            //TODO: Find a way to avoid doing this. 
            //Used to detect if the nodes already has been merged, as the list contains doublets 
            HashSet<Vector2> newNodes = new HashSet<Vector2>();
            //Those nodes should be deleted after the loop
            List<Vector2> deleteNodes = new List<Vector2>();
            //Merges the nodes which were close together. 
            foreach(Vector2Pair vp in nodesToMerge)
            {
                //The new node point should be in between the two
                Vector2 newNodePos = Vector2.Lerp(vp.First, vp.Second, 0.5f);
                //Debug.Log(newNodePos);
                if(newNodes.Add(newNodePos))
                {
                    Debug.Log(vp.First + " " + vp.Second);
                    TileNode newNode = new TileNode(newNodePos, new LineDebugger());
                    PolyGrid.Add(newNodePos, newNode);
                    //Updates each edge to point to the new location. 
                    foreach (Vector2 edge in PolyGrid[vp.First])
                    {
                        TileNode node = PolyGrid[edge];
                        node.RemoveEdge(vp.First);
                        node.AddEdge(newNodePos);
                        //Shouldn't reference the pair which we are about to remove
                        if(edge!=vp.Second)
                            newNode.AddEdge(edge);
                    }
                    foreach(Vector2 edge in PolyGrid[vp.Second])
                    {
                        TileNode node = PolyGrid[edge];
                        node.RemoveEdge(vp.First);
                        node.AddEdge(newNodePos);
                        if(edge!=vp.First)
                            newNode.AddEdge(edge);
                    }
                deleteNodes.Add(vp.First);
                deleteNodes.Add(vp.Second);

                }

            }
            foreach(Vector2 v in deleteNodes)
            {
                PolyGrid.Remove(v);
            }
            Debug.Log("Culled: " + deleteNodes.Count);

            Debug.Log("Casted " + rayCounter + " rays on " + PolyGrid.Count + " vertices");


            //Uses the generated grid to find a path: 
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
                        if(Vector2.Distance(Value.Key, neighbour) < MergeDistance)
                        {
                            //TODO: Nothing is drawn in red for w/e reason
                            Gizmos.color = Color.red;
                        }
                        else
                        {
                            Gizmos.color = Color.magenta;
                        }
                        Gizmos.DrawLine(Value.Key, neighbour);
                    }
                }
            }
        }
   


}
