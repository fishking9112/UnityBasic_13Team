using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public Action OnAttackEnd;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsDamage = Animator.StringToHash("IsDamage");
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();

    }

    public void Move(Vector3 obj)
    {
        animator.SetBool(IsMoving, obj.magnitude > 0.5f);
    }
    public void Attack()
    {
        animator.SetBool(IsAttack, true);
    }
    public void Attack(bool Attack)
    {
        animator.SetBool(IsAttack, Attack);
    }
    public void AttackAniEnd()
    {
        OnAttackEnd.Invoke();
    }
    public void Damage()
    {
        animator.SetBool(IsDamage, true);
    }
    public void InvincibilityEnd()
    {
        animator.SetBool(IsDamage, false);
    }

    public void Dead()
    {
        animator.SetBool(IsDead, true);
    }

}
