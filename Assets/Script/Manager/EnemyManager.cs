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
}
