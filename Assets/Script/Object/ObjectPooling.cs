using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class ObjectPooling : MonoBehaviour
{
    private ObjectPool<GameObject> playerPool;
    private ObjectPool<GameObject> enemyPool;
    private const int maxValue = 100;
    private const int initSize = 30;
    private const int enemyInitSize = 5;
    private const int enemyMaxValue = 20;


    [SerializeField] private GameObject playerObj;
    [SerializeField] private GameObject enemyObj;
    [SerializeField] private Transform parent;
    [SerializeField] private Transform enemyParent;

    private void Awake()
    {
        // 필요한 참조가 모두 설정되었는지 확인
        if (playerObj == null)
        {
            Debug.LogError("playerObj가 할당되지 않았습니다. Inspector에서 할당해주세요.");
        }
        
        if (enemyObj == null)
        {
            Debug.LogError("enemyObj가 할당되지 않았습니다. Inspector에서 할당해주세요.");
        }
        
        if (parent == null)
        {
            Debug.LogWarning("parent가 할당되지 않았습니다. 현재 Transform을 사용합니다.");
            parent = transform;
        }
        
        if (enemyParent == null)
        {
            Debug.LogWarning("enemyParent가 할당되지 않았습니다. 현재 Transform을 사용합니다.");
            enemyParent = transform;
        }
    }

    private void Start()
    {
        // null 체크 후 풀 초기화
        if (playerObj != null && parent != null)
        {
            playerPool = new ObjectPool<GameObject>(CreateObject, ActivatePoolObject,
                DisablePoolObject, DestroyPoolObject, false, initSize, maxValue);
        }
        
        if (enemyObj != null && enemyParent != null)
        {
            enemyPool = new ObjectPool<GameObject>(CreateEnemyObject, ActivatePoolObject,
                DisablePoolObject, DestroyPoolObject, false, enemyInitSize, enemyMaxValue);
        }
    }

    private GameObject CreateObject()
    {
        return Instantiate(playerObj, parent);
    }

    private GameObject CreateEnemyObject()
    {
        return Instantiate(enemyObj, enemyParent);
    }

    private void ActivatePoolObject(GameObject _obj)
    {
        _obj.SetActive(true);
    }
    private void DisablePoolObject(GameObject obj) 
    {
        obj.SetActive(false);
    }

    private void DestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }
    public GameObject GetObject(int bulletIndex)
    {
        switch (bulletIndex)
        {
            case 0:
                return GetPlayerObj();
            case 1:
                return GetEnemyObj();
            default:
                Debug.LogWarning($"알 수 없는 bulletIndex: {bulletIndex}");
                return null;
        }
    }
    
    public void ReleaseObject(GameObject obj, int bulletIndex)
    {
        if (obj == null) return;
        
        switch (bulletIndex)
        {
            case 0:
                ReleasePlayerObj(obj);
                break;
            case 1:
                ReleaseEnemyObj(obj);
                break;
            default:
                Debug.LogWarning($"알 수 없는 bulletIndex: {bulletIndex}");
                break;
        }
    }

    private void ReleasePlayerObj(GameObject obj)
    {
        if (obj.CompareTag("PoolOverObj"))
        {
            Destroy(obj);
        }
        else if (playerPool != null)
        {
            playerPool.Release(obj);
        }
    }
    
    private void ReleaseEnemyObj(GameObject obj)
    {
        if (obj.CompareTag("PoolOverObj"))
        {
            Destroy(obj);
        }
        else if (enemyPool != null)
        {
            enemyPool.Release(obj);
        }
    }
    
    private GameObject GetPlayerObj()
    {
        // playerPool이 초기화되지 않았으면 null 반환
        if (playerPool == null)
        {
            Debug.LogError("playerPool이 초기화되지 않았습니다. playerObj가 Inspector에서 할당되었는지 확인하세요.");
            return null;
        }
        
        GameObject sel = null;

        // maxSize를 넘는다면 임시 객체 생성 및 반환
        if (playerPool.CountActive >= maxValue)
        {
            sel = CreateObject();
            sel.tag = "PoolOverObj";
        }
        else
        {
            sel = playerPool.Get();
        }
        return sel;
    }
    
    private GameObject GetEnemyObj()
    {
        // enemyPool이 초기화되지 않았으면 null 반환
        if (enemyPool == null)
        {
            Debug.LogError("enemyPool이 초기화되지 않았습니다. enemyObj가 Inspector에서 할당되었는지 확인하세요.");
            return null;
        }
        
        GameObject sel = null;

        // maxSize를 넘는다면 임시 객체 생성 및 반환
        if (enemyPool.CountActive >= enemyMaxValue)
        {
            sel = CreateEnemyObject();
            sel.tag = "PoolOverObj";
        }
        else
        {
            sel = enemyPool.Get();
        }
        return sel;
    }
}
