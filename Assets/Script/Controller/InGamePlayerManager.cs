using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인게임에서 플레이어의 스탯과 상태를 관리하는 클래스
/// </summary>
public class InGamePlayerManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static InGamePlayerManager _instance;
    public static InGamePlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InGamePlayerManager");
                _instance = go.AddComponent<InGamePlayerManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 스탯 참조
    private TotalStatsReader statsReader;

    // 현재 스탯 값
    private int _maxHealth;
    private int _currentHealth;
    private int _attack;
    private int _defense;
    private float _damageReduction;
    private float _criticalRate;
    private float _moveSpeed;

    // 스탯 프로퍼티
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public int Attack => _attack;
    public int Defense => _defense;
    public float DamageReduction => _damageReduction;
    public float CriticalRate => _criticalRate;
    public float MoveSpeed => _moveSpeed;

    // 이벤트
    public delegate void HealthChangedHandler(int currentHealth, int maxHealth);
    public event HealthChangedHandler OnHealthChanged;

    private void Awake()
    {
        // 싱글톤 설정
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // TotalStatsReader 참조 가져오기
        statsReader = TotalStatsReader.Instance;
        
        // 초기 스탯 로드
        LoadStats();
    }

    /// <summary>
    /// TotalStatsReader에서 스탯을 로드합니다.
    /// </summary>
    public void LoadStats()
    {
        if (statsReader == null)
        {
            statsReader = TotalStatsReader.Instance;
        }

        // 스탯 로드
        _maxHealth = statsReader.MaxHealth;
        _attack = statsReader.Attack;
        _defense = statsReader.Defense;
        _damageReduction = statsReader.DamageReduction;
        _criticalRate = statsReader.CriticalRate;
        _moveSpeed = statsReader.MoveSpeed;

        // 현재 체력 초기화 (게임 시작 시 최대 체력으로 설정)
        _currentHealth = _maxHealth;

        Debug.Log($"플레이어 스탯 로드 완료: 체력 {_maxHealth}, 공격력 {_attack}, 방어력 {_defense}");
    }

    /// <summary>
    /// 현재 체력을 변경합니다.
    /// </summary>
    /// <param name="amount">변경할 양 (음수는 데미지, 양수는 회복)</param>
    public void ChangeHealth(int amount)
    {
        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);

        // 체력이 변경되었을 때만 이벤트 발생
        if (previousHealth != _currentHealth)
        {
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            Debug.Log($"플레이어 체력 변경: {previousHealth} -> {_currentHealth}");
            // UI에 플레이어 체력 반영할 자리
        }

        // 체력이 0이 되면 사망 처리
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 데미지를 받습니다. 방어력과 방어율이 적용됩니다.
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    public void TakeDamage(int damage)
    {
        // 방어력으로 데미지 감소
        int reducedDamage = Mathf.Max(1, damage - _defense);
        
        // 방어율로 데미지 추가 감소
        reducedDamage = Mathf.RoundToInt(reducedDamage * (1f - _damageReduction));
        
        // 최소 1의 데미지는 입도록 설정
        reducedDamage = Mathf.Max(1, reducedDamage);
        
        // 체력 감소
        ChangeHealth(-reducedDamage);
        
        Debug.Log($"플레이어가 데미지를 받음: 원래 데미지 {damage}, 감소된 데미지 {reducedDamage}");
    }

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    /// <param name="amount">회복할 양</param>
    public void Heal(int amount)
    {
        ChangeHealth(amount);
        Debug.Log($"플레이어 체력 회복: +{amount}");
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log("플레이어 사망");
        // 여기에 사망 관련 로직 추가
        // 예: 게임 오버 UI 표시, 애니메이션 재생 등
    }

    /// <summary>
    /// 공격 데미지를 계산합니다. 크리티컬 확률이 적용됩니다.
    /// </summary>
    /// <returns>최종 공격 데미지</returns>
    public int CalculateAttackDamage()
    {
        // 크리티컬 확률 계산
        bool isCritical = Random.value < _criticalRate;
        
        // 크리티컬이면 데미지 1.5배
        int damage = isCritical ? Mathf.RoundToInt(_attack * 1.5f) : _attack;
        
        Debug.Log($"공격 데미지 계산: 기본 {_attack}, 크리티컬 {isCritical}, 최종 데미지 {damage}");
        return damage;
    }

    /// <summary>
    /// 스탯을 새로고침합니다. 장비 변경 등으로 TotalStats.json이 업데이트된 후 호출하세요.
    /// </summary>
    public void RefreshStats()
    {
        // TotalStatsReader에서 스탯 새로고침
        statsReader.RefreshStats();
        
        // 이전 최대 체력 저장
        int previousMaxHealth = _maxHealth;
        
        // 스탯 다시 로드
        _maxHealth = statsReader.MaxHealth;
        _attack = statsReader.Attack;
        _defense = statsReader.Defense;
        _damageReduction = statsReader.DamageReduction;
        _criticalRate = statsReader.CriticalRate;
        _moveSpeed = statsReader.MoveSpeed;
        
        // 최대 체력이 증가한 경우, 현재 체력도 그만큼 증가
        if (_maxHealth > previousMaxHealth)
        {
            _currentHealth += (_maxHealth - previousMaxHealth);
        }
        // 최대 체력이 감소한 경우, 현재 체력이 최대 체력을 넘지 않도록 조정
        else if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
        
        // 체력 변경 이벤트 발생
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        Debug.Log("플레이어 스탯 새로고침 완료");
    }

    /// <summary>
    /// 현재 스탯 정보를 문자열로 반환합니다.
    /// </summary>
    public string GetStatsInfoString()
    {
        return $"체력: {_currentHealth}/{_maxHealth}\n" +
               $"공격력: {_attack}\n" +
               $"방어력: {_defense}\n" +
               $"방어율: {_damageReduction * 100:F1}%\n" +
               $"크리티컬율: {_criticalRate * 100:F1}%\n" +
               $"이동속도: {_moveSpeed:F1}";
    }
} 