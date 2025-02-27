using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene_Number { ACT_1 = 3 , ACT_2 , ACT_3 , ACT_4 , ACT_BOSS}

public class GameSceneManager : MonoBehaviour
{
    public GameManager gameManager;
    public Scene_Number curScene;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void LoadNextScene(Scene_Number _NextScene)
    {
        StartCoroutine(LoadMyAsyncScene(_NextScene));
    }
    public void LoadNextScene()
    {
        StartCoroutine(LoadMyAsyncScene());
    }

    //public void LoadNextScene(string SceneName)
    //{
    //    StartCoroutine(LoadMyAsyncScene(SceneName));
    //}

    //IEnumerator LoadMyAsyncScene(string SceneName)
    //{
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);

    //    while(!asyncLoad.isDone)
    //    {
    //        gameManager.Init();

    //        yield return null ;
    //    }
    //}
    IEnumerator LoadMyAsyncScene(Scene_Number _NextScene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)_NextScene , LoadSceneMode.Single);

        curScene = _NextScene;

        while (!asyncLoad.isDone)
        {
            gameManager.Init();

            yield return null;
        }

    }

    IEnumerator LoadMyAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)curScene + 1, LoadSceneMode.Single);

        curScene ++;

        while (!asyncLoad.isDone)
        {
            gameManager.Init();

            yield return null;
        }

    }

}
