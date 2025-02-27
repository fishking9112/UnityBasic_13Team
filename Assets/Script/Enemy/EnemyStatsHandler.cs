using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsHandler : MonoBehaviour
{
    [System.Serializable]
    public class EnemyStats
    {
        public int maxHealth = 100;
        public int attack = 10;
        public int defense = 5;
        public float damageReduction = 0.1f;
        public float moveSpeed = 3f;
    }

    [Header("Enemy Stats")]
    [SerializeField] private EnemyStats baseStats;
    
    // 현재 스탯 값
    private int currentHealth;
    
    // 스탯 프로퍼티
    public int MaxHealth => baseStats.maxHealth;
    public int CurrentHealth => currentHealth;
    public int Attack => baseStats.attack;
    public int Defense => baseStats.defense;
    public float DamageReduction => baseStats.damageReduction;
    public float MoveSpeed => baseStats.moveSpeed;

    // 이벤트
    public delegate void HealthChangedHandler(int currentHealth, int maxHealth);
    public event HealthChangedHandler OnHealthChanged;
    
    // 사망 이벤트
    public delegate void EnemyDeathHandler(EnemyController enemy);
    public event EnemyDeathHandler OnEnemyDeath;

    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        currentHealth = baseStats.maxHealth;
    }

    /// <summary>
    /// 현재 체력을 변경합니다.
    /// </summary>
    /// <param name="amount">변경할 양 (음수는 데미지, 양수는 회복)</param>
    /// <returns>사망 여부</returns>
    public bool ChangeHealth(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, baseStats.maxHealth);

        // 체력이 변경되었을 때만 이벤트 발생
        if (previousHealth != currentHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, baseStats.maxHealth);
            Debug.Log($"몬스터 체력 변경: {previousHealth} -> {currentHealth}");
        }

        // 체력이 0이 되면 사망 처리
        if (currentHealth <= 0)
        {
            // 즉시 이벤트 호출
            if (enemyController != null)
            {
                OnEnemyDeath?.Invoke(enemyController);
            }
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 데미지를 받습니다. 방어력과 방어율이 적용됩니다.
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    /// <returns>사망 여부</returns>
    public bool TakeDamage(int damage)
    {
        // 방어력으로 데미지 감소
        int reducedDamage = Mathf.Max(1, damage - baseStats.defense);
        
        // 방어율로 데미지 추가 감소
        reducedDamage = Mathf.RoundToInt(reducedDamage * (1f - baseStats.damageReduction));
        
        // 최소 1의 데미지는 입도록 설정
        reducedDamage = Mathf.Max(1, reducedDamage);
        
        // 체력 감소
        bool isDead = ChangeHealth(-reducedDamage);
        
        Debug.Log($"몬스터가 데미지를 받음: 원래 데미지 {damage}, 감소된 데미지 {reducedDamage}");
        
        return isDead;
    }

    /// <summary>
    /// 몬스터 스탯을 설정합니다. (레벨에 따른 스탯 조정 등에 사용)
    /// </summary>
    public void SetStats(int health, int attack, int defense, float damageReduction, float moveSpeed)
    {
        baseStats.maxHealth = health;
        baseStats.attack = attack;
        baseStats.defense = defense;
        baseStats.damageReduction = damageReduction;
        baseStats.moveSpeed = moveSpeed;
        
        currentHealth = health;
    }
} 