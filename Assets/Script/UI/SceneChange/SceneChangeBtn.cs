using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//씬 전환 버튼스크립트 

public class SceneChangeBtn : MonoBehaviour
{
        // 게임씬 이동
    public void StartGameScene()
    {
        SceneManager.LoadScene("Test_DunGeon_Scene"); 
    }

        // 메인씬 이동
    public void GoHomeScene()
    {
        SceneManager.LoadScene("Main_Scene");
    }
}
