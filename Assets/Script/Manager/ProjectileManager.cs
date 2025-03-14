using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private static ProjectileManager instance;
    public static ProjectileManager Instance { get { return instance; } }

    [SerializeField] private GameObject[] projectilePrefabs;

    [SerializeField] private ParticleSystem impactParticleSystem;

    private void Awake()
    {
        instance = this;
    }

    public void ShootBullet(RangeWeaponHandler rangeWeaponHandler,Vector3 startPosition, Vector3 direction)
    {
        GameObject origin = projectilePrefabs[rangeWeaponHandler.BulletIndex];

        float rotZ = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        GameObject obj = GameManager.Instance.objectPooling.GetObject(rangeWeaponHandler.BulletIndex);
        obj.transform.position = startPosition;
        obj.transform.rotation = Quaternion.Euler(0, rotZ, 0);

        ProjectileController projectileController = obj.GetComponent<ProjectileController>();
        projectileController.Init(direction, rangeWeaponHandler,this);

    }

    public void CreateImpactParticleAtPosition(Vector3 position, RangeWeaponHandler weaponHandler)
    {
        impactParticleSystem.transform.position = position;
        ParticleSystem.EmissionModule em = impactParticleSystem.emission;
        em.SetBurst(0, new ParticleSystem.Burst(0, Mathf.Ceil(weaponHandler.BulletSize * 5)));

        ParticleSystem.MainModule mainModule = impactParticleSystem.main;
        mainModule.startSpeedMultiplier = weaponHandler.BulletSize * 10f;
        impactParticleSystem.Play();
    }

}
