using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class PlayerController : BaseController
{
    private Camera camera;
    private GameManager gameManager;

    [SerializeField]
    private OnScreenStick stick;

    //private GameManager gameManager;

    private EnemyController nearestEnemy;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        camera=Camera.main;
    }


    protected override void HandleAction()
    {

        FindNearestEnemy();
        //float horiaontal = Input.GetAxisRaw("Horizontal");
        //float vertical= Input.GetAxisRaw("Vertical");
        //movementDirection=new Vector3(horiaontal,0, vertical).normalized;

        //Vector2 mousePosition = Input.mousePosition;
        //Vector2 worldPos = camera.ScreenToWorldPoint(mousePosition);


        //lookDirection = (worldPos - (Vector2)transform.position);

        //if (lookDirection.magnitude < .9f)
        //{
        //    lookDirection = Vector2.zero;
        //}
        //else
        //{
        //    lookDirection = lookDirection.normalized;
        //}

        //isAttacking = Input.GetMouseButton(0);


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

    private void FindNearestEnemy()
    {
        RaycastHit[] hit= Physics.BoxCastAll(transform.position, new Vector3(10, 1, 10).normalized, transform.forward, transform.rotation,
            10, 1 << LayerMask.NameToLayer("Enemy"));
        if(hit.Length>0)
        {
            Debug.Log(hit.Length);
        }

    }

    private void OnDrawGizmos()
    {
        RaycastHit[] hit = Physics.BoxCastAll(transform.position, new Vector3(100, 10, 100).normalized, transform.forward, transform.rotation,
            10, 1 << LayerMask.NameToLayer("Enemy"));
        Debug.Log(hit.Length);
        if (hit.Length > 0)
        {
            Gizmos.DrawWireCube(transform.position + transform.forward * hit[0].distance, transform.lossyScale);
        }
        else
        {
            Gizmos.DrawRay(transform.position, transform.forward * 10);
        }
    }


}
