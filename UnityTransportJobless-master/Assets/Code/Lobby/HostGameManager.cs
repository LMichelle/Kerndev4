using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostGameManager : MonoBehaviour
{
    public GameObject grid;
    private void Start()
    {
        Instantiate(grid);
    }
}
