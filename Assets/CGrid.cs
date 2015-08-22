using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Contains information about the grid. Can be used by pathfinders to show debug info. 
/// </summary>
class CGrid
    {
        public UnityEngine.Object Square;
        public UnityEngine.Object Arrow;
    
    public CGrid(Vector2 b)
    {
        Bounds = b;
    }

    public CTile this[Vector2 i]
    {
        private set
        {

        }
        get
        {
            return Grid[i];
        }
    }
    public Dictionary<Vector2, CTile> Grid
        {
            private set;
            get;
        }
    public static int TileSize = 32;
    public Vector2 Bounds
    {
        private set;
        get;
    }

    public void Draw()
    {
        Grid = new Dictionary<Vector2, CTile>();
        Vector2 tilePos = new Vector2(0, 0);
        for (; tilePos.y < Bounds.y; ++tilePos.y)
        {
            Vector2 worldPos = new Vector2(0, TileSize * tilePos.y);
            tilePos.x = 0;

            for (; tilePos.x < Bounds.x; ++tilePos.x)
            {
                worldPos.x = TileSize * tilePos.x;
                GameObject tile = GameObject.Instantiate(Square, (Vector3)worldPos, Quaternion.identity) as GameObject;
                GameObject arrow = GameObject.Instantiate(Arrow, (new Vector2(TileSize/2, TileSize/2)), Quaternion.identity) as GameObject;
                tile.name = "Square " + tilePos.ToString();
                CTile gridObject = new CTile(tile, arrow);
                Grid[tilePos] = gridObject;

                Debug.Log(worldPos);
            }

        }

    }
    public void Reset()
    {
        foreach (var n in Grid)
        { 
            n.Value.ResetDisplay();
        }
    }


}

