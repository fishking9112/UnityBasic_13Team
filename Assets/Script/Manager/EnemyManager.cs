using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyController> activeEnemies = new List<EnemyController>();


    private GameManager gameManager;
    private void Awake()
    {
        gameManager = transform.parent.GetComponent<GameManager>();
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




}
