using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 매니저 사용

public class SceneChangeBtn : MonoBehaviour
{
    public void StartGameScene()
    {
        SceneManager.LoadScene("Test_DunGeon_Scene"); 
    }

    public void GoHomeScene()
    {
        SceneManager.LoadScene("Main_Scene");
    }
}
