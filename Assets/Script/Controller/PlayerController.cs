using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class PlayerController : BaseController
{
    private Camera camera;
    private GameManager gameManager;

    [SerializeField]
    private OnScreenStick stick;

    //private GameManager gameManager;

    private Transform nearestEnemy;

    private LayerMask enemyLayer;

    private Vector3 overlapSize;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        camera=Camera.main;

        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        overlapSize = new Vector3(10, 1, 10);

    }


    protected override void HandleAction()
    {

        FindNearestEnemy();
     
       

    }

    public override void Death()
    {
        base.Death();

        //gameManager.GameOver();
    }

    private void OnMove(InputValue value)
    {
        Vector2 v=value.Get<Vector2>();
        movementDirection = lookDirection = new Vector3(v.x,0,v.y);
    }

    private void a()
    {
        RaycastHit[] hit = Physics.BoxCastAll(transform.position, overlapSize, transform.forward, transform.rotation,
    0, enemyLayer);
        if (hit.Length > 0)
        {
            for(int i=0;i<hit.Length;i++)
            {
                Debug.Log(i +" : "+Vector3.Distance(transform.position, hit[i].transform.position));

            }


            nearestEnemy = hit[0].transform;

        }

    }

    private void FindNearestEnemy()
    {
        Collider[] hit = Physics.OverlapBox(transform.position, overlapSize, Quaternion.identity, enemyLayer);
        if(hit.Length > 0 )
        {
            (int, float) min = (0,100f);

            for (int i = 0; i < hit.Length; i++)
            { 
                Vector3 dir = hit[i].transform.position;
                RaycastHit ray;
                Physics.Raycast(transform.position, dir - transform.position, out ray);
                if (ray.transform.gameObject.layer != 10)
                    continue;
                
                if(min.Item2>ray.distance)
                {
                    min.Item1 = i;
                    min.Item2 = ray.distance;
                }

            }
            nearestEnemy = min.Item2 == 0 ? null : hit[min.Item1].transform;
            LookNearestEnemy();
        }
    }



    private void LookNearestEnemy()
    {
        lookDirection = (nearestEnemy.position - transform.position);

        if (lookDirection.magnitude < .9f)
        {
            lookDirection = Vector3.zero;
        }
        else
        {
            lookDirection = lookDirection.normalized;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
//        RaycastHit[] hit = Physics.BoxCastAll(transform.position, overlapSize, transform.forward, transform.rotation,
//0, enemyLayer);
        //Debug.Log(Vector3.Distance(transform.position,hit.First().transform.position));

        //Gizmos.DrawWireCube(transform.position, overlapSize);
        Gizmos.DrawWireCube(transform.position, overlapSize);
    }
}
