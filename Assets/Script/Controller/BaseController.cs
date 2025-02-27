using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlExtensions;

public class BaseController : MonoBehaviour
{
    protected enum State
    {
        Move,
        Attack,
        Idle,
        Dead
    }

    protected Rigidbody _rigidbody;

    protected Vector3 movementDirection = Vector3.zero;
    public Vector3 MovementDirection { get { return movementDirection; } }

    protected Vector3 lookDirection = Vector3.zero;
    public Vector3 LookDirection { get { return lookDirection; } }

    protected AnimationHandler animationHandler;

    [SerializeField] public WeaponHandler WeaponPrefab;
    protected WeaponHandler weaponHandler;

    protected bool isAttacking;

    [SerializeField]
    protected State enumState;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        animationHandler = GetComponent<AnimationHandler>();

        weaponHandler = GetComponentInChildren<WeaponHandler>();
    }

    protected virtual void Start()
    {
        enumState = State.Idle;
    }

    protected virtual void Update()
    {
        switch (enumState)
        {
            case State.Idle:

                break;
            case State.Move:
                Rotate(lookDirection);
                break;
            case State.Attack:
                Rotate(lookDirection);
                HandleAction();
                break;
            case State.Dead:
                break;
        }
    }

    protected virtual void FixedUpdate()
    {
        switch (enumState)
        {
            case State.Idle:

                break;
            case State.Move:
                MoveMent(MovementDirection);
                break;
            case State.Attack:
                break;
            case State.Dead:
                break;
        }
    }
    protected virtual void HandleAction()
    {

    }

    private void MoveMent(Vector3 direction)
    {
        float moveSpeed = 3f;
        
        if (this is PlayerController)
        {
            moveSpeed = InGamePlayerManager.Instance.MoveSpeed;
        }
        else if (this is EnemyController)
        {
            moveSpeed = ((EnemyController)this).GetMoveSpeed();
        }
        
        direction = direction * moveSpeed;
       
        _rigidbody.velocity = direction;
        animationHandler.Move(direction);
    }

    private void Rotate(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        float rotZ = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        _rigidbody.rotation= Quaternion.Euler(0f, rotZ,0f );
    }


    protected virtual void Attack()
    {
        if (lookDirection != Vector3.zero)
        {
            weaponHandler?.Attack();
            animationHandler.Attack();
            isAttacking = false;
        }
    }

    public virtual void Death()
    {
        _rigidbody.velocity = Vector3.zero;

        foreach (SpriteRenderer renderer in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = renderer.color;
            color.a = 0.3f;
            renderer.color = color;

        }

        foreach (Behaviour component in transform.GetComponentsInChildren<Behaviour>())
        {
            component.enabled = false;
        }

        Destroy(gameObject, 2f);


    }

    public virtual void TakeDamage(float changed)
    {

    }

    public virtual int GetPower()
    {
        if (this is PlayerController)
        {
            return InGamePlayerManager.Instance.Attack;
        }
        else if (this is EnemyController && GetComponent<EnemyStatsHandler>() != null)
        {
            return GetComponent<EnemyStatsHandler>().Attack;
        }
        
        return 10;
    }

}
