using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
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
    private float timeSinceLastAttack = float.MaxValue;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        animationHandler = GetComponent<AnimationHandler>();
        statHandler = GetComponent<StatHandler>();

        weaponHandler = GetComponentInChildren<WeaponHandler>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        HandleAction();
        Rotate(lookDirection);
        HandleAttackDelay();
    }

    protected virtual void FixedUpdate()
    {
        MoveMent(MovementDirection);

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

        _rigidbody.rotation = Quaternion.Euler(0f, rotZ, 0f);
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

    protected virtual void Attack()
    {
        if (lookDirection != Vector3.zero)
        {
            weaponHandler?.Attack();
            animationHandler.Attack();
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
