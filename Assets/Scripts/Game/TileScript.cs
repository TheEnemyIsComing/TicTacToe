using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileScript : MonoBehaviour
{
    public Vector2 coords;

    private GameObject FigureModel;
    private TileState state = TileState.Free;

    public TileState State
    {
        get { return (state); }
        set
        {
            if (FigureModel != null)
                Destroy(FigureModel);
            if (value != TileState.Free)
            {
                if (value == TileState.X)
                    FigureModel = Instantiate(Resources.Load("Prefabs/X")) as GameObject;
                else
                    FigureModel = Instantiate(Resources.Load("Prefabs/O")) as GameObject;
                FigureModel.transform.position = gameObject.transform.position;
            }
            state = value;
        }
    }

    public void SetWinColor()
    {
        //FigureModel.GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/WinMat") as Material;
        foreach (Transform t in FigureModel.transform)
        {
            t.GetComponent<Renderer>().sharedMaterial = Resources.Load("Materials/WinMat") as Material;
        }
    }
}

public enum TileState { Free, X, O }