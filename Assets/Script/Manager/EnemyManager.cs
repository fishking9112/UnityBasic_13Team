using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;

public class EnemyManager : MonoBehaviour
{
    private Coroutine waveRoutine;

    [SerializeField] private List<GameObject> enemyPrefabs;

    [SerializeField] List<Rect> spawnAreas;
    [SerializeField] private Color gizmoColor = new Color(1, 0, 0, 0.3f);

    public List<EnemyController> activeEnemies = new List<EnemyController>();

    private bool enemySpawnComplite;

    #pragma warning disable 0414
    [SerializeField] private float timeBetweenSpawns = 0.2f;
    [SerializeField] private float timeBetweenWaves = 1f;
    #pragma warning restore 0414

    private GameManager gameManager;
    private void Awake()
    {
        gameManager = transform.parent.GetComponent<GameManager>();
    }

    private void OnDrawGizmosSelected()
    {
        if(spawnAreas==null)
        {
            return;
        }

        Gizmos.color = gizmoColor;

        foreach(var area in spawnAreas)
        {
            Vector3 center = new Vector3(area.x + area.width / 2, area.y + area.height / 2);
            Vector3 size = new Vector3(area.width, area.height);

            Gizmos.DrawCube(center, size);
        }
    }

    public void RemoveEnemyOnDeath(EnemyController enemy)
    {
        activeEnemies.Remove(enemy);

        Debug.Log("남은 " + enemy.name + " : " + activeEnemies.Count);

        if(activeEnemies.Count==0)
        {
            gameManager.OpenNextDungeon();
        }
    }

    private void SpawnWave()
    {
        StartCoroutine(SpawnEnemiesWithDelay());
    }

    private IEnumerator SpawnEnemiesWithDelay()
    {
        Debug.Log($"적 웨이브 생성: 스폰 간격 {timeBetweenSpawns}초, 웨이브 간격 {timeBetweenWaves}초");
        
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        
        yield return new WaitForSeconds(timeBetweenWaves);
    }

    public void SpawnEnemy(Vector3 position, int enemyType)
    {
        // 적 프리팹이 없거나 인덱스가 범위를 벗어나면 오류 처리
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("적 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        if (enemyType < 0 || enemyType >= enemyPrefabs.Count)
        {
            Debug.LogWarning($"유효하지 않은 적 타입: {enemyType}, 기본 타입(0)으로 대체합니다.");
            enemyType = 0;
        }
        
        // 적 생성
        GameObject enemyPrefab = enemyPrefabs[enemyType];
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        // 적 컨트롤러 초기화
        EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            // 플레이어를 타겟으로 설정
            enemyController.Init(this, GameManager.Instance.player.transform);
            
            // 활성 적 목록에 추가
            activeEnemies.Add(enemyController);
            
            // InGameUIManager에 등록
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.RegisterEnemy(enemyController);
                Debug.Log($"EnemyManager: {newEnemy.name} 생성 및 UI 매니저에 등록 완료");
            }
            else
            {
                Debug.LogWarning("InGameUIManager 인스턴스를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError($"생성된 적 {newEnemy.name}에 EnemyController 컴포넌트가 없습니다.");
        }
    }
}
