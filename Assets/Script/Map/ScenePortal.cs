using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    GameSceneManager gameSceneManager;

    private void Awake()
    {
        gameSceneManager = GetComponent<GameSceneManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == null) return;

        if ( other.transform.CompareTag("Player"))
        {
            Debug.Log("Player Enter");
        }
    }
}
