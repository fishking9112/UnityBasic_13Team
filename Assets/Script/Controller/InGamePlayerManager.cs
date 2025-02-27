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

    // 현재 스탯 값 - 인스펙터에서 볼 수 있도록 [SerializeField] 추가
    [Header("현재 스탯 (읽기 전용)")]
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _attack;
    [SerializeField] private int _defense;
    [SerializeField] private float _damageReduction;
    [SerializeField] private float _criticalRate;
    [SerializeField] private float _moveSpeed;

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

    // 스탯 변경 이벤트 추가
    public delegate void StatsChangedHandler();
    public event StatsChangedHandler OnStatsChanged;

    [Header("디버그")]
    [SerializeField] private bool _debugMode = false;
    [SerializeField] private string _lastActionLog = "";
    
    // 테스트용 데미지 필드 추가
    [Header("테스트")]
    [SerializeField] private int _testDamageAmount = 10;

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

        // 스탯 초기화
        InitializeStats();
    }

    /// <summary>
    /// 초기 스탯을 설정합니다.
    /// </summary>
    private void InitializeStats()
    {
        // 기본 스탯 설정 (나중에 TotalStatsReader에서 로드)
        _maxHealth = 100;
        _currentHealth = 100;
        _attack = 10;
        _defense = 5;
        _damageReduction = 0.1f;
        _criticalRate = 0.05f;
        _moveSpeed = 5f;
        
        if (_debugMode)
        {
            _lastActionLog = "스탯 초기화 완료";
            Debug.Log(_lastActionLog);
        }
    }

    private void Start()
    {
        // TotalStatsReader 참조 가져오기
        statsReader = TotalStatsReader.Instance;
        
        // 초기 스탯 로드
        if (statsReader != null)
        {
            LoadStats();
        }
        else
        {
            Debug.LogError("TotalStatsReader 인스턴스를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// TotalStatsReader에서 스탯을 로드합니다.
    /// </summary>
    public void LoadStats()
    {
        if (statsReader == null)
        {
            statsReader = TotalStatsReader.Instance;
            
            if (statsReader == null)
            {
                Debug.LogError("TotalStatsReader 인스턴스를 찾을 수 없습니다.");
                return;
            }
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

        if (_debugMode)
        {
            _lastActionLog = $"플레이어 스탯 로드 완료: 체력 {_maxHealth}, 공격력 {_attack}, 방어력 {_defense}";
            Debug.Log(_lastActionLog);
        }
        
        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
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
            
            if (_debugMode)
            {
                _lastActionLog = $"플레이어 체력 변경: {previousHealth} -> {_currentHealth}";
                Debug.Log(_lastActionLog);
            }
            // UI에 플레이어 체력 반영할 자리
        }

        // 체력이 0이 되면 사망 처리
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 데미지를 계산합니다. 방어력과 방어율이 적용됩니다.
    /// </summary>
    /// <param name="incomingDamage">받을 원래 데미지</param>
    /// <returns>방어력과 방어율이 적용된 최종 데미지</returns>
    private int CalculateDamage(int incomingDamage)
    {
        // 방어력으로 데미지 감소
        int reducedDamage = Mathf.Max(1, incomingDamage - _defense);
        
        // 방어율로 데미지 추가 감소
        reducedDamage = Mathf.RoundToInt(reducedDamage * (1f - _damageReduction));
        
        // 최소 1의 데미지는 입도록 설정
        return Mathf.Max(1, reducedDamage);
    }

    /// <summary>
    /// 데미지를 받습니다.
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    /// <returns>사망 여부</returns>
    public bool TakeDamage(int damage)
    {
        // 데미지 계산 (방어력 적용)
        int finalDamage = CalculateDamage(damage);
        
        // 체력 감소
        _currentHealth = Mathf.Max(0, _currentHealth - finalDamage);
        
        // 디버그 로그
        if (_debugMode)
        {
            _lastActionLog = $"데미지 받음: {damage} -> 최종 {finalDamage}, 남은 체력: {_currentHealth}/{_maxHealth}";
            Debug.Log(_lastActionLog);
        }
        
        // 체력 변경 이벤트 발생
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        // 사망 처리
        if (_currentHealth <= 0)
        {
            Die();
        }
        
        // 사망 여부 반환
        return _currentHealth <= 0;
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
        if (statsReader == null)
        {
            statsReader = TotalStatsReader.Instance;
            
            if (statsReader == null)
            {
                Debug.LogError("TotalStatsReader 인스턴스를 찾을 수 없습니다.");
                return;
            }
        }
        
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
        
        if (_debugMode)
        {
            _lastActionLog = "플레이어 스탯 새로고침 완료";
            Debug.Log(_lastActionLog);
        }
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

    // 디버그용 메서드 추가
    [ContextMenu("체력 회복")]
    private void DebugHeal()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        _lastActionLog = "체력 완전 회복";
        Debug.Log(_lastActionLog);
    }

    [ContextMenu("데미지 테스트 (10)")]
    private void DebugDamage()
    {
        TakeDamage(10);
    }

    // 테스트용 데미지 메서드 추가
    [ContextMenu("플레이어에게 테스트 데미지 주기")]
    public void ApplyTestDamage()
    {
        TakeDamage(_testDamageAmount);
        Debug.Log($"테스트 데미지 적용: {_testDamageAmount}");
    }
    
    // 에디터에서 버튼으로 사용할 메서드 추가
    public void ApplyDamage5()
    {
        TakeDamage(5);
        Debug.Log("테스트 데미지 적용: 5");
    }
    
    public void ApplyDamage10()
    {
        TakeDamage(10);
        Debug.Log("테스트 데미지 적용: 10");
    }
    
    public void ApplyDamage50()
    {
        TakeDamage(50);
        Debug.Log("테스트 데미지 적용: 50");
    }
} 