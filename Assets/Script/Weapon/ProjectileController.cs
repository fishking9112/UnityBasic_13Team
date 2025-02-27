using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    enum SPECIAL_EFFECT
    {
        NORMAL=1,
        REFRACTION=2,   // 굴절
        BOOM=4          // 폭발
    }

    [SerializeField] private LayerMask levelCollisionLayer;

    private RangeWeaponHandler rangeWeaponHandler;

    private float currentDuration;
    private Vector3 direction;
    private bool isReady;

    private Rigidbody _rigidbody;

    public bool fxOnDestroy = false;

    private ProjectileManager projectileManager;

    private int sp;

    // 튕기는 화살 튕기는 횟수
    private int reflectCount;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        sp = 1;
        reflectCount = 1;
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
        // 이미 죽은 적인지 확인
        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null && enemy.IsDead())
        {
            // 이미 죽은 적이면 무시
            return;
        }
        
        // 벽에 부딫힐 경우
        if (levelCollisionLayer.value == (levelCollisionLayer.value | (1 << collision.gameObject.layer)))
        {
            // 튕기는 화살일 경우, 튕기는 횟수 체크
            if(sp==(sp|1<<(int)SPECIAL_EFFECT.REFRACTION) && reflectCount>0)
            {
                Vector3 income = direction; 

                RaycastHit ray;
                Physics.Raycast(transform.position, direction, out ray);
                Vector3 normal = ray.normal;
                direction = Vector3.Reflect(income, normal);

                float rotZ = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0,rotZ,0);

                reflectCount--;
            }
            else
            {
                // 튕기는 화살이 아닐 경우, 튕김 횟수가 다 된 경우 파괴
                DestroyProjectile(collision.ClosestPoint(transform.position) - direction * 0.2f, fxOnDestroy);
            }
        }
        // 타겟(적)에게 부딫힐 경우
        else if (rangeWeaponHandler.target.value == (rangeWeaponHandler.target.value | (1 << collision.gameObject.layer)))
        {
            // 특수 효과 넣을 부분(폭발 등)
            if (sp == (sp | 1 << (int)SPECIAL_EFFECT.BOOM))
            {
                // 폭발 이펙트 및 폭발데미지 추가 필요
                DestroyProjectile(collision.ClosestPoint(transform.position), fxOnDestroy);
            }
            else
            {
                // 폭발 아니면 그냥 파괴
                DestroyProjectile(collision.ClosestPoint(transform.position), fxOnDestroy);
            }
            
            // 데미지 적용 전 로그 출력
            Debug.Log($"투사체 데미지 적용: {rangeWeaponHandler.Power}");
            
            // 상대 피격 처리
            collision.GetComponent<BaseController>().TakeDamage(rangeWeaponHandler.Power);
        }
    }

    public void Init(Vector3 direction, RangeWeaponHandler weaponHandler, ProjectileManager projectileManager)
    {
        this.projectileManager = projectileManager;
        
        rangeWeaponHandler = weaponHandler;

        this.direction = direction;
        currentDuration = 0;
        transform.localScale = Vector3.one * weaponHandler.BulletSize;
        sp += rangeWeaponHandler.specailAbility;
        
        // 투사체 초기화 시 공격력 로그 출력
        Debug.Log($"투사체 초기화: 공격력 {rangeWeaponHandler.Power}");

        isReady = true;
    }

    private void DestroyProjectile(Vector3 position,bool createFx)
    {
        if(createFx)
        {
            projectileManager.CreateImpactParticleAtPosition(position, rangeWeaponHandler);
        }
        GameManager.Instance.objectPooling.ReleaseObject(this.gameObject,rangeWeaponHandler.BulletIndex);
        //Destroy(this.gameObject);
    }
}
