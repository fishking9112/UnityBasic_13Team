using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RangeWeaponHandler : WeaponHandler
{
    [Header("Ranged Attack Data")]
    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private int bulletIndex;

    public int BulletIndex { get { return bulletIndex; } }


    [SerializeField] private float bulletSize = 1f;
    public float BulletSize { get { return bulletSize; } }
    [SerializeField] private float duration;
    public float Duration { get { return duration; } }

    [SerializeField] private float spread;
    public float Spread { get { return spread; } }

    [SerializeField] private int numberofProjectilePerShot;
    public int NumberofProjectilePerShot { get { return numberofProjectilePerShot; } }
    [SerializeField] private float multipleProjectileAngle;
    public float MultipleProjectileAngle { get { return multipleProjectileAngle; } }

    [SerializeField] private Color projectileColor;
    public Color ProjectileColor { get { return projectileColor; } }

    public int specailAbility;


    private ProjectileManager projectileManager;

    protected override void Start()
    {
        base.Start();
        projectileManager = ProjectileManager.Instance;
        specailAbility = 1;
    }

    public override void Attack()
    {
        base.Attack();

        float projectileAngleSpace = multipleProjectileAngle;
        int numberOfProjectilePerShot = numberofProjectilePerShot;

        float minAngle = -(numberOfProjectilePerShot / 2f) * projectileAngleSpace;

        for(int i=0;i<numberOfProjectilePerShot;i++)
        {
            float angle = projectileAngleSpace *(i-1);
            CreateProjectile(Controller.LookDirection, angle);
        }
    }


    private void CreateProjectile(Vector3 _lookDirection, float angle)
    {
        projectileManager.ShootBullet(this, projectileSpawnPosition.position, RotateVector3(_lookDirection,angle));
    }

    private static Vector3 RotateVector3(Vector3 v, float degree)
    {
        return Quaternion.Euler(0, 0, degree) * v;
    }

    // 화살 능력 추가할 때 호출
    public override void SetAbility(int index)
    {
        switch (index)
        {
            case 2:
            case 4:
                specailAbility += index;
                break;
            case 1:
                numberofProjectilePerShot = 3;
                break;

        }
    }


}
