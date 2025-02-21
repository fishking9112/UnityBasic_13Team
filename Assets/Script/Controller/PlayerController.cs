using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : BaseController
{
    private Camera camera;

    //private GameManager gameManager;

    public void Init()
    {
        //this.gameManager = gameManager;
        camera=Camera.main;
    }


    protected override void HandleAction()
    {
        float horiaontal = Input.GetAxisRaw("Horizontal");
        float vertical= Input.GetAxisRaw("Vertical");
        movementDirection=new Vector3(horiaontal,0, vertical).normalized;

        //Vector2 mousePosition = Input.mousePosition;
        //Vector2 worldPos = camera.ScreenToWorldPoint(mousePosition);
        //lookDirection = (worldPos - (Vector2)transform.position);

        //if(lookDirection.magnitude<.9f)
        //{
        //    lookDirection = Vector2.zero;
        //}
        //else
        //{
        //    lookDirection = lookDirection.normalized;
        //}

        isAttacking = Input.GetMouseButton(0);
    
    
    }

    public override void Death()
    {
        base.Death();
        //gameManager.GameOver();
    }

}
