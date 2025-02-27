using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene_Number { Main = 2, ACT_1 = 3, ACT_2 , ACT_3 , ACT_4 }
public class GameSceneManager : MonoBehaviour
{
    public GameManager gameManager;

    public Scene_Number NextScene;

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

    //오버로딩
    public void LoadNextScene()
    {
        StartCoroutine(LoadMyAsyncScene());
    }
    IEnumerator LoadMyAsyncScene()
    {
        int NextSceneNumber = (int)NextScene;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(NextSceneNumber, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            //gameManager.Init();

            yield return null;
        }
    }

}
