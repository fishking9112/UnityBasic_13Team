using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlExtensions;

public class EnemyController : BaseController
{
    private EnemyManager enemyManager;
    private Transform target;
    private EnemyStatsHandler statsHandler;

    [SerializeField] public float followRange = 15f;
    private float timeSinceLastAttack = float.MaxValue;

    // 사망 이벤트 추가
    public delegate void EnemyDeathHandler(EnemyController enemy);
    public event EnemyDeathHandler OnEnemyDeath;

    protected override void Awake()
    {
        base.Awake();
        
        // statsHandler가 없으면 추가
        statsHandler = GetComponent<EnemyStatsHandler>();
        if (statsHandler == null)
        {
            Debug.LogWarning("EnemyStatsHandler not found on " + gameObject.name + ". Adding one.");
            statsHandler = gameObject.AddComponent<EnemyStatsHandler>();
        }
        
        if (statsHandler != null)
        {
            statsHandler.OnEnemyDeath += HandleDeath;
        }
    }

    public void Init(EnemyManager enemyManager, Transform target)
    {
        this.enemyManager = enemyManager;
        this.target = target;
    }

    protected override void Update()
    {
        base.Update();

        switch (enumState)
        {
            case State.Idle:
                FindTarget();
                break;
            case State.Move:
                HandleAttackDelay();
                break;
            case State.Attack:
                HandleAction();
                break;
            case State.Dead:
                break;
        }

    }

    private void FindTarget()
    {
        if (weaponHandler == null || target == null)
        {
            if (!movementDirection.Equals(Vector3.zero))
                movementDirection = Vector3.zero;
            return;
        }

        float distance = DistanceToTarget();
        Vector3 direction = DirectionToTarget();

        if(distance<=weaponHandler.AttackRange)
        {
            enumState = State.Attack;

        }

        else if (distance <= followRange)
        {
            lookDirection = direction;
            enumState = State.Move;
        }
    }

    protected float DistanceToTarget()
    {
        return Vector3.Distance(transform.position, target.position);
    }

    protected Vector3 DirectionToTarget()
    {
        Vector3 target2DPos = target.position;
        target2DPos.y = 0f;
        Vector3 transform2DPos = transform.position;
        transform2DPos.y = 0f;

        return (target2DPos - transform2DPos).normalized;

        //return (target.position - transform.position).normalized;

        /*
         * 3D 좌표계로 방향벡터를 구했을 때 , Y 위치값에 따라 방향벡터의 Y값이 달라 질 수 있어서
         * Y값을 0으로 만든 뒤 , 노말벡터를 만들어 방향벡터를 구해준다.
         */
    }

    protected override void HandleAction()
    {
        base.HandleAction();

        float distance = DistanceToTarget();
        Vector3 direction = DirectionToTarget();

        if (distance < weaponHandler.AttackRange && isAttacking == true)
        {
            Debug.Log("Att !! ");
            isAttacking = false;
            StopMoving();
        }
        else if (distance > weaponHandler.AttackRange)
        {
            enumState = State.Move;
            movementDirection = direction;
            animationHandler.Attack(false);
        }
    }

    private void HandleDeath(EnemyController enemy)
    {
        // 이미 Dead 상태면 중복 처리 방지
        if (enumState == State.Dead)
            return;
        
        enumState = State.Dead;
        
        // 즉시 EnemyManager에 알림
        if (enemyManager != null)
        {
            enemyManager.RemoveEnemyOnDeath(this);
        }
        
        // 나머지 사망 처리는 TakeDamage에서 처리
        
        // 사망 이벤트 발생
        OnEnemyDeath?.Invoke(this);
    }

    public override void Death()
    {
        // 이제 HandleDeath에서 처리
    }

    private void HandleAttackDelay()
    {
        if (weaponHandler == null)
            return;

        movementDirection = DirectionToTarget();
        lookDirection = movementDirection;

        if (timeSinceLastAttack <= weaponHandler.Delay)
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        if (!isAttacking && timeSinceLastAttack > weaponHandler.Delay && DistanceToTarget() < weaponHandler.AttackRange)
        {
            timeSinceLastAttack = 0;
            StopMoving();
            Attack();
            enumState = State.Attack;
            return;
        }

        else if(DistanceToTarget() < weaponHandler.AttackRange)
        {
            enumState = State.Idle;

        }
    }

    private void StopMoving()
    {
        _rigidbody.velocity = Vector3.zero;
        animationHandler.Move(Vector3.zero);
    }

    public override void TakeDamage(float changed)
    {
        base.TakeDamage(changed);

        if (statsHandler == null)
        {
            Debug.LogError("statsHandler is null on " + gameObject.name);
            return;
        }

        bool isDead = statsHandler.TakeDamage(Mathf.RoundToInt(changed));

        if (isDead)
        {
            // 즉시 사망 처리
            animationHandler.Dead();
            
            // 즉시 처리할 부분
            _rigidbody.velocity = Vector3.zero;
            
            // 콜라이더 비활성화 (있다면)
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            
            // 투명도 변경
            foreach (SpriteRenderer renderer in transform.GetComponentsInChildren<SpriteRenderer>())
            {
                Color color = renderer.color;
                color.a = 0.3f;
                renderer.color = color;
            }
            
            // 사망 이벤트 발생 - 여기서 명시적으로 호출
            Debug.Log("몬스터 사망 이벤트 발생: " + gameObject.name);
            OnEnemyDeath?.Invoke(this);
            
            // 더 빠른 파괴 (0.5초 -> 0.2초)
            Destroy(gameObject, 0.2f);
        }
    }

    public float GetMoveSpeed()
    {
        return statsHandler != null ? statsHandler.MoveSpeed : 3f;
    }

    public bool IsDead()
    {
        return enumState == State.Dead;
    }
}
