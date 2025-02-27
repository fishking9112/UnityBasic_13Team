using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeBtn : MonoBehaviour
{
    [Header("씬 설정")]
    [Tooltip("이동할 씬 이름")]
    public string targetSceneName = "";  // 기본값 없음
    // 인스펙터에서 설정한 씬으로 이동
    public void GoToScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("이동할 씬 이름이 설정되지 않았습니다.");
        }
    }
}
