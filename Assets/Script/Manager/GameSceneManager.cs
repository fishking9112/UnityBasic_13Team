using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public GameManager gameManager;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void LoadNextScene(string SceneName)
    {
        StartCoroutine(LoadMyAsyncScene(SceneName));
    }
    IEnumerator LoadMyAsyncScene(string SceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);

        while(!asyncLoad.isDone)
        {
            gameManager.Init();

            yield return null ;
        }
    }

}
