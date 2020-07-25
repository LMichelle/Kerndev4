using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void ReloadScene()
    {
        Destroy(GameObject.FindGameObjectWithTag("Server"));
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
