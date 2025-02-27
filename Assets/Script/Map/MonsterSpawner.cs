using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Map 에서 몬스터를 소환 해 주는 스크립트
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject monsterSpawn;


    private EnemyManager enemyManager;
    private GameManager gameManager;

    [SerializeField] private List<GameObject> enemyPrefabs;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //Pos 찾아오기
        //자식 갯수만큼
        int spawnCount = monsterSpawn.transform.childCount;

        for(int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = monsterSpawn.transform.GetChild(i).transform.position;

            //만들기
            SpawnEnemy(pos);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnEnemy(Vector3 spawnPos)
    {
        if (enemyPrefabs.Count == 0 )
        {
            Debug.LogWarning("Enemy Prefab 가 설정되지 않았습니다.");
            return;
        }

        GameObject randomPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        GameObject spawnEnemy = Instantiate(randomPrefab, spawnPos, Quaternion.identity);

        EnemyController enemyController = spawnEnemy.GetComponent<EnemyController>();
        enemyController.Init(enemyManager, gameManager.player.transform);

        enemyManager.activeEnemies.Add(enemyController);
    }
}
