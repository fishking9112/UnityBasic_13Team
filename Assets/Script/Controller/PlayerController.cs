using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : BaseController
{
    private Camera _camera;
    private GameManager gameManager;

    private Transform nearestEnemy;

    private LayerMask enemyLayer;

    private Vector3 overlapSize;

    public bool isBattle;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        _camera=Camera.main;

        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        overlapSize = new Vector3(10, 1, 10);

        StartCoroutine(SearchTarget());
        StartCoroutine(LookTarget());
        StartCoroutine(StartAttack());
    }

    public void SearchEnemy()
    {
        StartCoroutine(SearchTarget());

    }

    IEnumerator LookTarget()
    {
        while(true)
        {
            yield return new WaitUntil(() => movementDirection == Vector3.zero);
            LookNearestEnemy();
        }
    }

    IEnumerator SearchTarget()
    {
        while(isBattle)
        {
            FindNearestEnemy();
            yield return new WaitUntil(() => nearestEnemy == null);
        }
    }

    protected override void HandleAction()
    {

        

    }

    public override void Death()
    {
        base.Death();

    }

    private void OnMove(InputValue value)
    {
        Vector2 v=value.Get<Vector2>();
        Vector3 v3 = new Vector3(v.x, 0, v.y);
        movementDirection = v3;
        lookDirection = v3;
    }

    private void FindNearestEnemyByCast()
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
        if (hit.Length > 0)
        {
            (int, float) min = (0, 100f);

            for (int i = 0; i < hit.Length; i++)
            {
                Vector3 dir = hit[i].transform.position;
                RaycastHit ray;
                Physics.Raycast(transform.position, dir - transform.position, out ray);
                if (ray.transform.gameObject.layer != 10)
                    continue;

                if (min.Item2 > ray.distance)
                {
                    min.Item1 = i;
                    min.Item2 = ray.distance;
                }

            }
            nearestEnemy = min.Item2 == 100 ? null : hit[min.Item1].transform;
        }
    }

    private void LookNearestEnemy()
    {
        if (nearestEnemy == null)
            return;

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

    protected override void Attack()
    {
        base.Attack();
    }

    IEnumerator StartAttack()
    {
        while(true)
        {
            yield return new WaitWhile(() => nearestEnemy==null);
            yield return new WaitUntil(() => movementDirection == Vector3.zero);
            yield return new WaitForSeconds(1);
            // 발사
            Attack();
        }
    }

}
