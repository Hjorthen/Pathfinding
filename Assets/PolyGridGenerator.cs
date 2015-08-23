using UnityEngine;
using System.Linq;
using System.Collections.Generic;



public class PolyGridGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {



	}
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        BoxCollider2D[] colliders = GameObject.FindObjectsOfType<BoxCollider2D>();
        Vector2[] corners = { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1) };
        List<Vector2> Vertices = new List<Vector2>();

        foreach (BoxCollider2D collider in colliders)
        {

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
                Vertices.Add(worldPos);
                Gizmos.DrawSphere(worldPos, 5);

            }


        }
        Debug.Log("Found " + colliders.Length + " colliders");

        //Loops over each vertex and raycasts to the other vertices to check if they have LOS
        foreach(Vector2 vertex in Vertices)
        {
            foreach (Vector2 otherVertex in Vertices)
            {
                //We dont want to check LOS for the current vertex
                if (otherVertex == vertex)
                    continue;
                RaycastHit2D ray;
                //If we didn't hit any objects between the two vertices then we can add it as a valid path
                if((ray = Physics2D.Linecast(vertex, otherVertex)).collider==null)
                {
                    Gizmos.DrawLine(vertex, otherVertex);
                }
            }

        }
    }

        // Update is called once per frame
        void Update () {
	
	}
}
