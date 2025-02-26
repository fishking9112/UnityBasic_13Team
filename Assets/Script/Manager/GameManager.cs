using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController player { get; private set; }

    [SerializeField] private int currentWaveIndex = 0;

    private EnemyManager enemyManager;

    public ObjectPooling objectPooling;

    public static bool isFirstLoading = true;

    private void Awake()
    {
        Instance = this;
        player = FindObjectOfType<PlayerController>();
        player.Init(this);


        enemyManager = GetComponentInChildren<EnemyManager>();
        enemyManager.Init(this);

        objectPooling = GetComponentInChildren<ObjectPooling>();

    }

    private void Start()
    {
       
    }

    public void StartGame()
    {
        StartNextWave();
    }

    void StartNextWave()
    {
        currentWaveIndex += 1;
        enemyManager.StartWave(1 + currentWaveIndex / 5);
    }

    public void EndOfWave()
    {
        StartNextWave();
    }

    public void GameOver()
    {
        enemyManager.StopWave();
    }


}
