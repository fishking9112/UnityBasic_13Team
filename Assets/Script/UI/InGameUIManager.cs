using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InGameUIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static InGameUIManager _instance;
    
    // 씬 전환 시 유지할 정적 데이터
    private static int s_playerLevel = 1;
    private static int s_currentExp = 0;
    private static int s_maxExp = 100;
    private static int s_gold = 0;
    private static bool s_isInitialized = false;
    
    public static InGameUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InGameUIManager>();
                
                if (_instance == null)
                {
                    Debug.LogError("씬에 InGameUIManager가 없습니다!");
                }
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
    [SerializeField] private Button resumeButton;

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

    // InGameUIManager.cs에 정적 변수 추가
    public static bool IsGamePaused { get; private set; } = false;

    // 중복 호출 방지를 위한 변수 추가
    private int lastToggleFrame = -1;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // 씬 전환 시 데이터 로드
        LoadPersistentData();

        // UI 컴포넌트 참조 확인
        if (pauseButton == null)
        {
            Debug.LogError("InGameUIManager: pauseButton이 Inspector에서 할당되지 않았습니다!");
        }
        
        if (pausePanel == null)
        {
            Debug.LogError("InGameUIManager: pausePanel이 Inspector에서 할당되지 않았습니다!");
        }

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

    // 씬 전환 시 데이터 로드
    private void LoadPersistentData()
    {
        if (s_isInitialized)
        {
            // 이미 초기화된 데이터가 있으면 로드
            playerLevel = s_playerLevel;
            currentExp = s_currentExp;
            maxExp = s_maxExp;
            gold = s_gold;
            Debug.Log($"이전 씬의 데이터 로드: 레벨 {playerLevel}, 경험치 {currentExp}/{maxExp}, 골드 {gold}");
        }
        else
        {
            // 첫 실행 시 초기값 설정
            s_isInitialized = true;
            s_playerLevel = playerLevel;
            s_currentExp = currentExp;
            s_maxExp = maxExp;
            s_gold = gold;
            Debug.Log("InGameUIManager 데이터 초기화");
        }
    }

    // 씬 전환 전 데이터 저장
    private void SavePersistentData()
    {
        s_playerLevel = playerLevel;
        s_currentExp = currentExp;
        s_maxExp = maxExp;
        s_gold = gold;
        Debug.Log($"데이터 저장: 레벨 {playerLevel}, 경험치 {currentExp}/{maxExp}, 골드 {gold}");
    }

    private void OnDestroy()
    {
        // 씬 전환 시 데이터 저장
        SavePersistentData();
    }

    private void Start()
    {
        // 기존 코드...
        
        // 게임 시간 강제 정상화
        Time.timeScale = 1f;
        Debug.Log($"Start 메서드에서 Time.timeScale 강제 설정: {Time.timeScale}");
        
        // 일시정지 상태 초기화
        isPaused = false;
        
        // 일시정지 패널 강제 비활성화
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("Start 메서드에서 일시정지 패널 강제 비활성화");
        }
        
        // 필수 초기화 코드 추가
        InitializeUI();
        SetupButtonListeners();
        RegisterAllEnemies();
        CheckEventSystem();
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

    private void SetupButtonListeners()
    {
        // 일시정지 버튼 설정
        if (pauseButton != null)
        {
            // 기존 리스너 제거 후 다시 추가 (중요!)
            pauseButton.onClick.RemoveAllListeners();
            
            // 디버그 로그 추가
            Debug.Log("일시정지 버튼 리스너 설정 - 기존 리스너 제거됨");
            
            pauseButton.onClick.AddListener(TogglePause);
            
            // 버튼이 상호작용 가능한지 확인
            if (!pauseButton.interactable)
            {
                pauseButton.interactable = true;
                Debug.Log("일시정지 버튼 interactable 속성을 true로 설정했습니다.");
            }
            
            Debug.Log("일시정지 버튼 이벤트 연결 완료");
        }
        else
        {
            Debug.LogError("일시정지 버튼 참조가 없습니다! Inspector에서 할당해주세요.");
        }
        
        // 게임 재개 버튼 설정
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log("게임 재개 버튼 이벤트 연결 완료");
        }
        else
        {
            Debug.LogWarning("게임 재개 버튼 참조가 없습니다. 일시정지 패널 내에 'ResumeButton'을 추가하세요.");
        }
    }

    private void CheckEventSystem()
    {
        // EventSystem 확인
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.LogError("씬에 EventSystem이 없습니다! UI 이벤트가 작동하지 않을 수 있습니다.");
            
            // EventSystem 자동 생성 (선택사항)
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("EventSystem을 자동으로 생성했습니다.");
        }
    }

    /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        // 같은 프레임에서 중복 호출 방지
        if (Time.frameCount == lastToggleFrame)
        {
            Debug.Log("TogglePause 중복 호출 방지");
            return;
        }
        
        lastToggleFrame = Time.frameCount;
        
        try
        {
            Debug.Log("TogglePause 메서드 호출됨");
            
            isPaused = !isPaused;
            IsGamePaused = isPaused; // 정적 변수 업데이트
            
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
        catch (System.Exception e)
        {
            Debug.LogError($"TogglePause 메서드 실행 중 오류 발생: {e.Message}");
        }
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
        
        // 데이터 저장
        SavePersistentData();
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
        
        // 데이터 저장
        SavePersistentData();
        
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
        
        // 데이터 저장
        SavePersistentData();
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

    // Update 메서드 수정
    private void Update()
    {
        // 주기적으로 timeScale 확인
        if (Time.frameCount % 100 == 0)
        {
            Debug.Log($"현재 Time.timeScale: {Time.timeScale}");
        }
        
        // 주기적으로 적 이벤트 연결 확인
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            RegisterAllEnemies();
        }
        
        // 이미 일시정지 토글 중이면 추가 입력 무시
        if (Time.frameCount == lastToggleFrame)
        {
            return;
        }
        
        // Input System 오류 방지를 위한 조건부 실행
        try
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("ESC 키 입력 감지 - 일시정지 토글");
                TogglePause();
                lastToggleFrame = Time.frameCount;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Input System 오류: {e.Message}");
            
            // 레거시 입력 시스템으로 폴백
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("레거시 입력 시스템: ESC 키 입력 감지");
                TogglePause();
                lastToggleFrame = Time.frameCount;
            }
        }
    }

    // OnEnable 메서드 수정
    private void OnEnable()
    {
        // 씬 로드 시 UI 참조 다시 찾기
        if (pauseButton == null)
        {
            pauseButton = GameObject.Find("PauseButton")?.GetComponent<Button>();
            if (pauseButton != null)
            {
                // 여기서 SetupButtonListeners 호출 제거
                // 대신 버튼이 null이 아닌지만 확인
                Debug.Log("OnEnable에서 일시정지 버튼 참조 찾음");
            }
        }
    }
} 