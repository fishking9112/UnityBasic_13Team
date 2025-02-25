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

    public bool fxOnDestroy = false;

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


    private void OnTriggerEnter(Collider collision)
    {
        // 寒俊 何婬鳃 版快
        if (levelCollisionLayer.value == (levelCollisionLayer.value | (1 << collision.gameObject.layer)))
        {
            DestroyProjectile(collision.ClosestPoint(transform.position) - direction * 0.2f, fxOnDestroy);

        }
        // 鸥百(利)俊霸 何婬鳃 版快
        else if (rangeWeaponHandler.target.value == (rangeWeaponHandler.target.value | (1 << collision.gameObject.layer)))
        {
            //ResourceController resourceController = collision.GetComponent<ResourceController>();
            //if (resourceController != null)
            {
                //    resourceController.ChangeHealth(-rangeWeaponHandler.Power);
            }
            DestroyProjectile(collision.ClosestPoint(transform.position), fxOnDestroy);
        }

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
