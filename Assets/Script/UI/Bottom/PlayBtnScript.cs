using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // �� �Ŵ��� ���

public class PlayBtnScript : MonoBehaviour
{
    public void StartGameScene()
    {
        SceneManager.LoadScene("Test_DunGeon_Scene");
        //Main Scene�� Game Scene�� ���� ���� �����´�.
        //������ ���۵ȴ�.
        //�÷��̾ ������ ReviveUI�� 5�ʵ��� ȣ���ϰ� �� �ڿ� GameOverUI�� ȣ���Ѵ�.
        //Tap�ϸ� Main���� ��������.
    }
}
