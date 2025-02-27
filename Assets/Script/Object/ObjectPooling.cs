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

    private void Start()
    {
        playerPool = new ObjectPool<GameObject>(CreateObject, ActivatePoolObject,
            DisablePoolObject, DestroyPoolObject, false, initSize,maxValue);
        enemyPool = new ObjectPool<GameObject>(CreateEnemyObject, ActivatePoolObject,
            DisablePoolObject, DestroyPoolObject, false, enemyInitSize, enemyMaxValue);
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
                return null;
        }
       
    
    }
    public void ReleaseObject(GameObject obj, int bulletIndex)
    {
        switch (bulletIndex)
        {
            case 0:
                ReleasePlayerObj(obj);
                break;
            case 1:
                ReleaseEnemyObj(obj);
                break;
            default:
                break;
        }
        
    }

    private void ReleasePlayerObj(GameObject obj)
    {
        if (obj.CompareTag("PoolOverObj"))
        {
            Destroy(obj);
        }
        else
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
        else
        {
            enemyPool.Release(obj);
        }
    }
    private GameObject GetPlayerObj()
    {
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
        GameObject sel = null;

        // maxSize를 넘는다면 임시 객체 생성 및 반환
        if (enemyPool.CountActive >= enemyMaxValue)
        {
            sel = CreateObject();
            sel.tag = "PoolOverObj";
        }
        else
        {
            sel = enemyPool.Get();
        }
        return sel;

    }
}
