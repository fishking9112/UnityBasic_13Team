using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static InGameUIManager _instance;
    public static InGameUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InGameUIManager");
                _instance = go.AddComponent<InGameUIManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("UI 참조")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider expBar;
    [SerializeField] private Text levelText;
    [SerializeField] private Text goldText;
    [SerializeField] private Button pauseButton;

    [Header("플레이어 데이터")]
    [SerializeField] private int currentExp;
    [SerializeField] private int maxExp = 100;
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private int gold;

    // 프로퍼티
    public int CurrentExp => currentExp;
    public int MaxExp => maxExp;
    public int PlayerLevel => playerLevel;
    public int Gold => gold;

    // 게임 일시정지 상태
    private bool isPaused = false;

    private InGamePlayerManager playerManager;

    private float checkTimer = 0f;
    private float checkInterval = 5f; // 5초마다 확인

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

        // 플레이어 매니저 참조 가져오기
        playerManager = InGamePlayerManager.Instance;
        if (playerManager != null)
        {
            Debug.Log("InGameUIManager: 플레이어 매니저 참조 성공");
        }
        else
        {
            Debug.LogError("InGameUIManager: 플레이어 매니저 참조 실패");
        }
    }

    private void Start()
    {
        // UI 초기화
        InitializeUI();
        
        // 이벤트 리스너 등록 - 명시적으로 버튼 이벤트 연결
        if (pauseButton != null)
        {
            // 기존 리스너 제거 후 다시 추가
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
            Debug.Log("일시정지 버튼 이벤트 연결 완료");
        }
        else
        {
            Debug.LogError("일시정지 버튼 참조가 없습니다!");
        }
        
        // 몬스터 사망 이벤트 구독
        RegisterAllEnemies();
    }

    private void InitializeUI()
    {
        // 경험치 바 초기화
        if (expBar != null)
        {
            expBar.maxValue = maxExp;
            expBar.value = currentExp;
        }
        
        // 레벨 텍스트 초기화
        if (levelText != null)
        {
            levelText.text = $"Lv {playerLevel}";
        }
        
        // 골드 텍스트 초기화
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }
        
        // 일시정지 패널 초기 상태 설정
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        Debug.Log("TogglePause 메서드 호출됨");
        
        isPaused = !isPaused;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
            Debug.Log($"일시정지 패널 상태: {(isPaused ? "활성화" : "비활성화")}");
        }
        else
        {
            Debug.LogError("일시정지 패널 참조가 없습니다!");
        }
        
        // 게임 시간 조절
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log($"Time.timeScale 설정: {Time.timeScale}");
    }

    /// <summary>
    /// 경험치 추가
    /// </summary>
    public void AddExperience(int amount)
    {
        currentExp += amount;
        
        // 레벨업 체크
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }
        
        // UI 업데이트
        if (expBar != null)
        {
            expBar.value = currentExp;
        }
        
        Debug.Log($"경험치 획득: +{amount}, 현재 경험치: {currentExp}/{maxExp}");
    }

    /// <summary>
    /// 레벨업 처리
    /// </summary>
    private void LevelUp()
    {
        playerLevel++;
        
        // 다음 레벨 경험치 요구량 증가 (레벨당 20% 증가)
        maxExp = Mathf.RoundToInt(maxExp * 1.2f);
        
        // 경험치 바 최대값 업데이트
        if (expBar != null)
        {
            expBar.maxValue = maxExp;
        }
        
        // 레벨 텍스트 업데이트
        if (levelText != null)
        {
            levelText.text = $"Lv {playerLevel}";
        }
        
        Debug.Log($"레벨 업! 현재 레벨: {playerLevel}, 다음 레벨까지 필요 경험치: {maxExp}");
        
        // 여기에 레벨업 보상 로직 추가 (스탯 증가 등)
    }

    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        gold += amount;
        
        // UI 업데이트
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }
        
        Debug.Log($"골드 획득: +{amount}, 현재 골드: {gold}");
    }

    /// <summary>
    /// 몬스터 처치 시 보상 처리
    /// </summary>
    private void OnEnemyDefeated(EnemyController enemy)
    {
        Debug.Log($"InGameUIManager: {enemy.name} 처치 보상 처리 시작");
        
        // 경험치 보상 (50%~80% 랜덤)
        int expReward = Mathf.RoundToInt(maxExp * Random.Range(0.5f, 0.8f));
        AddExperience(expReward);
        
        // 골드 보상 (고정 150골드)
        AddGold(150);
    }

    /// <summary>
    /// 새로운 몬스터 등록 (동적으로 생성된 몬스터용)
    /// </summary>
    public void RegisterEnemy(EnemyController enemy)
    {
        if (enemy != null)
        {
            // 이미 구독 중인지 확인하기 위해 이벤트 구독 해제 후 다시 구독
            enemy.OnEnemyDeath -= OnEnemyDefeated;
            enemy.OnEnemyDeath += OnEnemyDefeated;
            Debug.Log($"InGameUIManager: {enemy.name} 몬스터 이벤트 구독 완료");
        }
    }

    /// <summary>
    /// 게임 재개 (일시정지 패널의 계속하기 버튼용)
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 게임 종료 (일시정지 패널의 게임 종료 버튼용)
    /// </summary>
    public void QuitGame()
    {
        // 게임 종료 전 필요한 저장 등의 작업 수행
        Debug.Log("게임 종료");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // 모든 적 등록 메서드 추가
    private void RegisterAllEnemies()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        Debug.Log($"InGameUIManager: {enemies.Length}개의 적 발견, 이벤트 구독 시작");
        
        foreach (var enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
    }

    // 디버그용 메서드 추가
    [ContextMenu("일시정지 토글 테스트")]
    private void TestTogglePause()
    {
        TogglePause();
    }

    // Update 메서드에 키보드 입력 추가
    private void Update()
    {
        // 주기적으로 적 이벤트 연결 확인
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            RegisterAllEnemies();
        }
        
        // ESC 키로 일시정지 토글 (디버그용)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC 키 입력 감지 - 일시정지 토글");
            TogglePause();
        }
    }
} 