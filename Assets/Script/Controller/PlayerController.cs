using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : BaseController
{
    private Camera _camera;
    private GameManager gameManager;

    private Transform nearestEnemy;

    private LayerMask enemyLayer;

    private Vector3 overlapSize;

    public bool isBattle;

    public AudioClip attackSoundClip;

    private InGamePlayerManager playerManager;
    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        playerManager = InGamePlayerManager.Instance;
        _camera =Camera.main;

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

    public override void Death()
    {
        base.Death();

    }

    private void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();

        Vector3 v3 = new Vector3(v.x, 0, v.y);
        movementDirection = v3;
        lookDirection = v3;

        if (v == Vector2.zero)
        {
            enumState = State.Idle;
            _rigidbody.velocity = movementDirection;
            animationHandler.Move(movementDirection);
        }
        else
            enumState = State.Move;

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


        if (movementDirection.magnitude > .9f)
        {
            lookDirection = movementDirection; ;
        }
        else
        {
            Vector3 target2DPos = nearestEnemy.position;
            target2DPos.y = 0f;
            Vector3 transform2DPos = transform.position;
            transform2DPos.y = 0f;

            lookDirection = (target2DPos - transform2DPos).normalized;

            //lookDirection = (nearestEnemy.position - transform.position).normalized;

            /*
             * 3D 좌표계로 방향벡터를 구했을 때 , Y 위치값에 따라 방향벡터의 Y값이 달라 질 수 있어서
             * Y값을 0으로 만든 뒤 , 노말벡터를 만들어 방향벡터를 구해준다.
             */
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
            // 일시정지 상태에서는 대기
            if (Time.timeScale <= 0.01f)
            {
                yield return null;
                continue;
            }
            
            yield return new WaitForSeconds(weaponHandler.Delay);
            yield return new WaitWhile(() => nearestEnemy==null);
            yield return new WaitUntil(() => movementDirection == Vector3.zero);
            // 발사
            enumState = State.Attack;
            LookNearestEnemy();
            Attack();

            if (attackSoundClip != null)
            {
                SoundManager.PlayClip(attackSoundClip);
            }
        }
    }


    private void GetSpecialAbility(int index)
    {
        weaponHandler.SetAbility(index);
        // 1=멀티샷 2=튕기는화살 3=폭발화살
    }

    public override void TakeDamage(float changed)
    {
        base.TakeDamage(changed);

        playerManager.TakeDamage(Mathf.RoundToInt(changed));
        
        if (enumState != State.Dead)
        {
            animationHandler.Damage();
        }
    }

    private void SubscribeToPlayerDeath()
    {
        playerManager.OnHealthChanged += (current, max) => {
            if (current <= 0 && enumState != State.Dead)
            {
                enumState = State.Dead;
                animationHandler.Dead();
            }
        };
    }

    protected override void Start()
    {
        base.Start();
        SubscribeToPlayerDeath();
    }

    protected override void Update()
    {
        // 일시정지 상태에서는 업데이트 중단
        if (Time.timeScale <= 0.01f)
            return;
        
        base.Update();
        // 나머지 코드...
    }
}
