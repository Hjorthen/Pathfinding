using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CTile
{
    public GameObject TileObject;
    private GameObject Arrow;

    public CTile(GameObject tile, GameObject A)
    {
        TileObject = tile;
        Arrow = A;
        Arrow.transform.SetParent(TileObject.transform, false);
    }


    private CTile() { throw new System.NotImplementedException(); }

    public Color color
    {
        get
        {
            SpriteRenderer render = TileObject.GetComponent<SpriteRenderer>();
            return render.color;
        }
        set
        {
            SpriteRenderer render = TileObject.GetComponent<SpriteRenderer>();
            render.color = value;
        }
    }

    public void PointArrow(float angle)
    {
        Arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetChecked()
    {
        color = Color.green;
    }

    public void SetStart()
    {
        color = Color.cyan;
    }

}

