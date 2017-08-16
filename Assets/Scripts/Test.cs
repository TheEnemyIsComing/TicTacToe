using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject t;
    void Start()
    {
        t.GetComponent<TileScript>().State = TileState.X;
    }

    void Update()
    {

    }
}