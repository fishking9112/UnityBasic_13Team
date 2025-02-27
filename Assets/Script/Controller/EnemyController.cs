using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlExtensions;

public class EnemyController : BaseController
{
    private EnemyManager enemyManager;
    private Transform target;

    [SerializeField] public float followRange = 15f;
    private float timeSinceLastAttack = float.MaxValue;

    public void Init(EnemyManager enemyManager,Transform target)
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


    public override void Death()
    {
        base.Death();
        enemyManager.RemoveEnemyOnDeath(this);
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

        bool isDead = statHandler.ChangeHealth(statHandler.CalculateDamaged((int)changed));

        if (isDead)
        {
            Death();
            animationHandler.Dead();
            Destroy(gameObject, 0.5f);
        }
    }
}
