using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CTile :  NodeDebugDisplay
{
    public GameObject TileObject;
    private GameObject Arrow;
    private SpriteRenderer SRenderer;
    //If set to true, then the colors should reset in the reset method
    private bool ShouldReset = true;

    public CTile(GameObject tile, GameObject arrow)
    {
        TileObject = tile;
        SRenderer = tile.GetComponent<SpriteRenderer>();
        Arrow = arrow;
        Arrow.transform.SetParent(TileObject.transform, false);
    }


    public void ShowArrow(bool val)
    {
            Arrow.SetActive(val);
    }
    private CTile() { throw new System.NotImplementedException(); }

    //Clears colors and makes the tile ready for showing a new path
    public void ResetDisplay()
    {
        if(ShouldReset)
            SetColor(Color.yellow);

        ShowArrow(false);
    }

    public void SetChecked()
    {
        SetColor(Color.green);
        ShouldReset = true;
    }

    public void SetStart()
    {
        SetColor(Color.cyan);
        ShouldReset = false;
    }

    public void SetWall()
    {
        SetColor(Color.gray);
        ShouldReset = false;
    }

    public void SetPath()
    {
            SetColor(Color.blue);
            ShouldReset = true;
    }

    public void SetColor(Color col)
    {
        SRenderer.color = col;
    }

    public void SetCostDisplay(string str) { 
                TextMesh txt = TileObject.GetComponentInChildren<TextMesh>();
                txt.text = str;
        }
    
    //Points an arrow in the direction of the angle
    public void SetPathDirection(float angle)
    {
        Arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

