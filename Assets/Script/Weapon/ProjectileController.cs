using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private LayerMask levelCollisionLayer;

    private RangeWeaponHandler rangeWeaponHandler;

    private float currentDuration;
    private Vector3 direction;
    private bool isReady;

    private Rigidbody _rigidbody;

    public bool fxOnDestroy = true;

    private ProjectileManager projectileManager;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!isReady) return;

        currentDuration += Time.deltaTime;

        if(currentDuration>rangeWeaponHandler.Duration)
        {
            DestroyProjectile(transform.position, false);
        }

        _rigidbody.velocity = direction * rangeWeaponHandler.Speed;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
       

            
            DestroyProjectile(collision.ClosestPoint(transform.position), fxOnDestroy);


    }


    public void Init(Vector3 direction,RangeWeaponHandler weaponHandler, ProjectileManager projectileManager)
    {
        this.projectileManager = projectileManager;

        rangeWeaponHandler = weaponHandler;
        float rotZ = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        this.transform.rotation = Quaternion.Euler(0f, -rotZ, 90f);
        this.direction = direction;
        currentDuration = 0;
        transform.localScale = Vector3.one * weaponHandler.BulletSize;


        isReady = true;
    }

    private void DestroyProjectile(Vector3 position,bool createFx)
    {
        if(createFx)
        {
            projectileManager.CreateImpactParticleAtPosition(position, rangeWeaponHandler);
        }

        Destroy(this.gameObject);
    }


}
