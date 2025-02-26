using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsDamage = Animator.StringToHash("IsDamage");
    private static readonly int IsAttack = Animator.StringToHash("IsAttack");

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

    public void Damage()
    {
        animator.SetBool(IsDamage, true);
    }
    public void InvincibilityEnd()
    {
        animator.SetBool(IsDamage, false);
    }


}
