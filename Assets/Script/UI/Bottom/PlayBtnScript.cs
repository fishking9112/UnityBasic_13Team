using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 매니저 사용

public class PlayBtnScript : MonoBehaviour
{
    public void StartGameScene()
    {
        SceneManager.LoadScene("Test_DunGeon_Scene");
        //Main Scene과 Game Scene의 같은 맵을 가져온다.
        //게임이 시작된다.
        //플레이어가 죽으면 ReviveUI를 5초동안 호출하고 그 뒤에 GameOverUI를 호출한다.
        //Tap하면 Main으로 나가진다.
    }
}
