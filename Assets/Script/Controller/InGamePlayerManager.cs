using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("충돌 데미지 설정")]
    [SerializeField] private bool enableCollisionDamage = true;
    [SerializeField] private int collisionDamage = 500;
    [SerializeField] private float damageInterval = 0.1f; // 연속 데미지 방지를 위한 간격
    [SerializeField] private bool ignoreDefenseForCollision = true; // 충돌 데미지에 방어력 무시 옵션
    private float lastDamageTime = 0f;
    
    // 플레이어 참조
    private PlayerController playerController;

    // 씬 로드 감지를 위한 변수 추가
    private string currentSceneName;
    private bool needsPlayerReconnect = false;

    [Header("사망 설정")]
    [SerializeField] private float deathDelay = 2.0f; // 사망 후 씬 전환까지의 지연 시간
    [SerializeField] private string mainSceneName = "Main_scene"; // 메인 씬 이름
    private bool isDead = false; // 사망 상태 플래그

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

        // 플레이어 컨트롤러 참조 찾기
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController를 찾을 수 없습니다. 충돌 데미지 처리가 작동하지 않을 수 있습니다.");
        }
        else
        {
            // 플레이어 콜라이더에 충돌 처리 스크립트 추가
            SetupCollisionDetection();
        }

        // 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 현재 씬 이름 저장
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    private void OnDestroy()
    {
        // 씬 전환 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬 로드 시 호출되는 이벤트 핸들러
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 변경되었는지 확인
        if (scene.name != currentSceneName)
        {
            currentSceneName = scene.name;
            Debug.Log($"새 씬 로드됨: {scene.name}, 플레이어 재연결 필요");
            
            // 플레이어 재연결 플래그 설정
            needsPlayerReconnect = true;
        }
    }
    
    private void Update()
    {
        // 플레이어 재연결이 필요하면 처리
        if (needsPlayerReconnect)
        {
            ReconnectPlayer();
        }
    }
    
    /// <summary>
    /// 씬 전환 후 플레이어 참조 재설정
    /// </summary>
    private void ReconnectPlayer()
    {
        // 플레이어 찾기 시도
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            
            if (playerController != null)
            {
                Debug.Log("새 씬에서 플레이어 컨트롤러 참조 찾음");
                
                // 충돌 감지 컴포넌트 재설정
                SetupCollisionDetection();
                
                // 재연결 완료
                needsPlayerReconnect = false;
            }
            else
            {
                // 플레이어가 아직 로드되지 않았을 수 있음, 다음 프레임에 다시 시도
                Debug.Log("플레이어 컨트롤러를 찾을 수 없음, 다음 프레임에 재시도");
            }
        }
        else
        {
            // 플레이어 참조는 있지만 충돌 감지 컴포넌트 확인
            PlayerCollisionHandler handler = playerController.GetComponent<PlayerCollisionHandler>();
            if (handler == null)
            {
                SetupCollisionDetection();
            }
            
            // 재연결 완료
            needsPlayerReconnect = false;
        }
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
            HandlePlayerDeath();
        }
    }

    /// <summary>
    /// 데미지를 계산합니다. 방어력과 방어율이 적용됩니다.
    /// </summary>
    private int CalculateDamage(int rawDamage)
    {
        // 원래 데미지 값 저장 (디버그용)
        int originalDamage = rawDamage;
        
        // 방어력에 따른 데미지 감소 (방어력 1당 데미지 1 감소)
        int reducedDamage = Mathf.Max(rawDamage - _defense, 1);
        
        // 방어율에 따른 데미지 감소 (퍼센트 기반)
        int finalDamage = Mathf.RoundToInt(reducedDamage * (1f - _damageReduction));
        
        // 최소 데미지는 1
        finalDamage = Mathf.Max(finalDamage, 1);
        
        // 디버그 로그 추가
        if (_debugMode || originalDamage > 10) // 중요한 데미지만 로그 출력
        {
            Debug.Log($"데미지 계산: 원본 {originalDamage} -> 방어력 적용 후 {reducedDamage} -> 최종 {finalDamage}");
        }
        
        return finalDamage;
    }

    /// <summary>
    /// 플레이어가 데미지를 입습니다.
    /// </summary>
    public bool TakeDamage(int amount)
    {
        // 이미 사망한 상태면 무시
        if (_currentHealth <= 0 || isDead)
        {
            return false;
        }
        
        // 원본 데미지 저장 (디버그용)
        int originalAmount = amount;
        
        // 충돌 데미지인 경우 방어력 적용 전 로그 출력
        if (amount == collisionDamage)
        {
            Debug.Log($"충돌 데미지 적용 전: {amount}");
        }
        
        // 데미지 계산 (방어력 및 방어율 적용)
        int calculatedDamage = CalculateDamage(amount);
        
        // 체력 변경
        ChangeHealth(-calculatedDamage);
        
        // 중요: 충돌 데미지인 경우 추가 로그 출력
        if (amount == collisionDamage)
        {
            Debug.Log($"충돌 데미지 처리: 원본 {originalAmount} -> 최종 {calculatedDamage}, 현재 체력: {_currentHealth}/{_maxHealth}");
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
    private void HandlePlayerDeath()
    {
        if (isDead) return; // 이미 사망 처리 중이면 무시
        
        isDead = true;
        Debug.Log("플레이어 사망 - 메인 씬으로 즉시 이동");
        
        // 아주 짧은 딜레이 후 씬 전환 (선택 사항)
        StartCoroutine(QuickReturnToMainScene());
    }

    /// <summary>
    /// 아주 짧은 딜레이 후 메인 씬으로 이동하는 코루틴
    /// </summary>
    private IEnumerator QuickReturnToMainScene()
    {
        // 매우 짧은 딜레이 (0.1초)
        yield return new WaitForSeconds(0.1f);
        
        // 게임 시간 정상화 (일시정지 상태일 수 있으므로)
        Time.timeScale = 1.0f;
        
        // 메인 씬으로 이동
        Debug.Log($"메인 씬({mainSceneName})으로 이동합니다.");
        SceneManager.LoadScene(mainSceneName);
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

    /// <summary>
    /// 플레이어 충돌 감지 설정
    /// </summary>
    private void SetupCollisionDetection()
    {
        if (playerController == null)
        {
            Debug.LogWarning("SetupCollisionDetection: 플레이어 컨트롤러 참조가 없습니다.");
            return;
        }
        
        // 플레이어 게임 오브젝트에 충돌 핸들러 추가
        PlayerCollisionHandler collisionHandler = playerController.gameObject.GetComponent<PlayerCollisionHandler>();
        if (collisionHandler == null)
        {
            collisionHandler = playerController.gameObject.AddComponent<PlayerCollisionHandler>();
        }
        
        // 충돌 이벤트 구독
        collisionHandler.OnEnemyCollision -= HandleEnemyCollision;
        collisionHandler.OnEnemyCollision += HandleEnemyCollision;
        
        Debug.Log("플레이어 충돌 감지 설정 완료");
    }
    
    /// <summary>
    /// 적과의 충돌 처리
    /// </summary>
    private void HandleEnemyCollision(EnemyController enemy)
    {
        // 충돌 데미지가 비활성화되어 있으면 무시
        if (!enableCollisionDamage)
        {
            return;
        }
        
        // 연속 데미지 방지를 위한 시간 체크
        if (Time.time - lastDamageTime < damageInterval)
        {
            return;
        }
        
        // 데미지 계산 및 적용
        int damage = collisionDamage;
        if (!ignoreDefenseForCollision)
        {
            // 방어력을 고려한 데미지 계산
            damage = CalculateDamageWithDefense(damage);
        }
        
        // 데미지 적용
        TakeDamage(damage);
        lastDamageTime = Time.time;
        
        // 디버그 로그
        if (_debugMode)
        {
            _lastActionLog = $"적 {enemy.name}과 충돌: {damage} 데미지";
            Debug.Log(_lastActionLog);
        }
    }

    // 방어력을 고려한 데미지 계산
    private int CalculateDamageWithDefense(int rawDamage)
    {
        // 방어력에 따른 데미지 감소 계산
        float damageReduction = _defense * 0.05f; // 방어력 1당 5%의 데미지 감소
        damageReduction = Mathf.Clamp(damageReduction, 0f, 0.7f); // 최대 70%까지 감소
        
        int reducedDamage = Mathf.RoundToInt(rawDamage * (1f - damageReduction));
        return Mathf.Max(1, reducedDamage); // 최소 1 데미지 보장
    }

    /// <summary>
    /// 플레이어의 공격력을 증가시킵니다.
    /// </summary>
    /// <param name="amount">증가시킬 공격력 값</param>
    public void IncreaseAttack(float amount)
    {
        // 플레이어 공격력 증가 로직 구현
        _attack += Mathf.RoundToInt(amount);
        Debug.Log($"플레이어 공격력 {amount} 증가, 현재 공격력: {_attack}");
        
        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어의 방어력을 증가시킵니다.
    /// </summary>
    /// <param name="amount">증가시킬 방어력 값</param>
    public void IncreaseDefense(float amount)
    {
        // 플레이어 방어력 증가 로직 구현
        _defense += Mathf.RoundToInt(amount);
        Debug.Log($"플레이어 방어력 {amount} 증가, 현재 방어력: {_defense}");
        
        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어의 최대 체력을 증가시킵니다.
    /// </summary>
    /// <param name="amount">증가시킬 최대 체력 값</param>
    public void IncreaseMaxHealth(float amount)
    {
        // 플레이어 최대 체력 증가 로직 구현
        int healthIncrease = Mathf.RoundToInt(amount);
        _maxHealth += healthIncrease;
        
        // 현재 체력도 함께 증가
        _currentHealth += healthIncrease;
        
        Debug.Log($"플레이어 최대 체력 {amount} 증가, 현재 최대 체력: {_maxHealth}, 현재 체력: {_currentHealth}");
        
        // 체력 변경 이벤트 발생
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어의 이동 속도를 증가시킵니다.
    /// </summary>
    /// <param name="amount">증가시킬 이동 속도 값</param>
    public void IncreaseMoveSpeed(float amount)
    {
        // 플레이어 이동 속도 증가 로직 구현
        _moveSpeed += amount;
        Debug.Log($"플레이어 이동 속도 {amount} 증가, 현재 이동 속도: {_moveSpeed}");
        
        // 플레이어 컨트롤러에 이동 속도 적용
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.UpdateMoveSpeed(_moveSpeed);
        }
        
        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 플레이어의 크리티컬 확률을 증가시킵니다.
    /// </summary>
    /// <param name="amount">증가시킬 크리티컬 확률 값 (0.0 ~ 1.0)</param>
    public void IncreaseCriticalChance(float amount)
    {
        // 플레이어 크리티컬 확률 증가 로직 구현
        _criticalRate += amount;
        Debug.Log($"플레이어 크리티컬 확률 {amount * 100}% 증가, 현재 크리티컬 확률: {_criticalRate * 100}%");
        
        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }
}

/// <summary>
/// 플레이어 충돌 감지 컴포넌트
/// </summary>
public class PlayerCollisionHandler : MonoBehaviour
{
    // 적과 충돌 시 발생하는 이벤트
    public delegate void EnemyCollisionHandler(EnemyController enemy);
    public event EnemyCollisionHandler OnEnemyCollision;
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void OnTriggerStay(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void HandleCollision(GameObject obj)
    {
        // 적 레이어 확인 (10번 레이어가 Enemy)
        if (obj.layer == 10)
        {
            EnemyController enemy = obj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                OnEnemyCollision?.Invoke(enemy);
            }
        }
    }
} 