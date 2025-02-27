using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        HandleAttackDelay();
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


        if(weaponHandler==null || target==null)
        {
            if (!movementDirection.Equals(Vector3.zero))
                movementDirection = Vector3.zero;
            return;
        }

        float distance = DistanceToTarget();
        Vector3 direction = DirectionToTarget();

        //isAttacking = false;

        if(distance <= followRange)
        {
            lookDirection = direction;

            if (distance < weaponHandler.AttackRange && isAttacking == false)
            {
                Debug.Log("Att !! ");
                isAttacking = true;

                movementDirection = Vector2.zero;

                return;
            }
            else
            {
                animationHandler.Attack(false);
            }

            movementDirection = direction;
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

        if (timeSinceLastAttack <= weaponHandler.Delay)
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        if (isAttacking && timeSinceLastAttack > weaponHandler.Delay)
        {
            timeSinceLastAttack = 0;
            Attack();
        }


    }


}
