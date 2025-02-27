using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController player { get; private set; }

    private EnemyManager enemyManager;

    public ObjectPooling objectPooling;

    public static bool isFirstLoading = true;

    public GameObject mapObject;

    private void Awake()
    {
        Instance = this;
        player = FindObjectOfType<PlayerController>();
        player.Init(this);

        enemyManager = GetComponentInChildren<EnemyManager>();

        objectPooling = GetComponentInChildren<ObjectPooling>();

        mapObject = GameObject.FindGameObjectWithTag("Map");

        //플레이어 초기 위치 정보 가져오기
        player.transform.position = mapObject.transform.Find("PlayerSpawn").position;
    }

    private void Start()
    {
       
    }



    public void OpenNextDungeon()
    {
        mapObject.transform.Find("Trigger").gameObject.SetActive(true);
    }
}
