using System.Collections;
using System.Collections.Generic;
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


    private ProjectileManager projectileManager;

    protected override void Start()
    {
        base.Start();
        projectileManager = ProjectileManager.Instance;
    }

    public override void Attack()
    {
        base.Attack();

        float projectileAngleSpace = multipleProjectileAngle;
        int numberOfProjectilePerShot = numberofProjectilePerShot;

        float minAngle = -(numberOfProjectilePerShot / 2f) * projectileAngleSpace;

        for(int i=0;i<numberOfProjectilePerShot;i++)
        {
            float angle = minAngle + projectileAngleSpace * i;
            float randomSpread = Random.Range(-spread, spread);
            angle += randomSpread;
            CreateProjectile(Controller.LookDirection, 0);
        }
    }


    private void CreateProjectile(Vector3 _lookDirection, float angle)
    {
        projectileManager.ShootBullet(this, projectileSpawnPosition.position, _lookDirection);
    }

    private static Vector2 RotateVector2(Vector3 v,float degree)
    {
        Vector3 v3 = new Vector3(v.x, 0, v.y);
        float rotZ = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;

        return Quaternion.Euler(0, rotZ,0 ) * v;
    }


}
