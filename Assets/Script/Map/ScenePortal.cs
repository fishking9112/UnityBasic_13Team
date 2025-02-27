using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    GameSceneManager gameSceneManager;

    private void Awake()
    {
        gameSceneManager = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == null) return;

        if ( other.transform.CompareTag("Player"))
        {
            Debug.Log("Player Enter");

            if (gameSceneManager != null)
            {
                gameSceneManager.LoadNextScene();
            }
            else
            {
                Debug.Log("gameSceneManager is null !");
            }



        }
    }
}
