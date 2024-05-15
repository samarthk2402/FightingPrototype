using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckForGameOver : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;

    IEnumerator LoadSceneAfterDelay()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2);

        // Load the specified scene
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        if(player==null || enemy==null){
            StartCoroutine(LoadSceneAfterDelay());
        }
    }
}
