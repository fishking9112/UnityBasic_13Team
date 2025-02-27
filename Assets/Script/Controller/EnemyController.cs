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
        return (target.position - transform.position).normalized;
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
}
