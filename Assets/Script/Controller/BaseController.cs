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
    protected StatHandler statHandler;

    [SerializeField] public WeaponHandler WeaponPrefab;
    protected WeaponHandler weaponHandler;

    protected bool isAttacking;

    protected State enumState;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        animationHandler = GetComponent<AnimationHandler>();
        statHandler = GetComponent<StatHandler>();

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
        direction = direction * statHandler.Speed;
       
        _rigidbody.velocity = direction;
        animationHandler.Move(direction);
    }

    private void Rotate(Vector3 direction)
    {
        // 회전값 0(입력값 없음)이면 바꾸지 않음
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

}
