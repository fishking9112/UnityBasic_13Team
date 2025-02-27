using System.Collections;
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController player { get; private set; }

    private EnemyManager enemyManager;

    public ObjectPooling objectPooling;

    public static bool isFirstLoading = true;

    private GameObject mapObject;

    public bool doorIsOpen = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Instance = this;
        player = FindObjectOfType<PlayerController>();

        enemyManager = GetComponentInChildren<EnemyManager>();

        objectPooling = GetComponentInChildren<ObjectPooling>();

        Init(); // 게임 매니저가 DontDestroyOnLoad 로 설정되기 때문에 , 씬이 넘어가도 바뀔 값은 Init 에서 따로 초기화 해준다.
    }


    
    public void Init()
    {
        //맵 정보 다시 가져오기.
        mapObject = GameObject.FindGameObjectWithTag("Map");

        //플레이어 초기 위치 정보 가져오기 ( 맵 마다 다르니까 새로 받아준다 )
        player.transform.position = mapObject.transform.Find("PlayerSpawn").position;

        // 문 단속
        doorIsOpen = false;
    }

    private void Start()
    {
        player.Init(this);
    }



    public void OpenNextDungeon()
    {
        if (!doorIsOpen)
        {
            doorIsOpen = true;
            
            Debug.Log("OpenNextDungeon !");

            GameObject.Find("Portal").transform.Find("Trigger").gameObject.SetActive(true);
        }
    }
}
