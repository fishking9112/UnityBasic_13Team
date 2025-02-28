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
    
    [Header("슬롯 머신 UI")]
    [SerializeField] private GameObject slotMachinePanel; // 슬롯 머신 패널
    [SerializeField] private SlotMachineMgr slotMachineMgr; // 슬롯 머신 매니저 참조
    [SerializeField] private AudioClip levelUpSound; // 레벨업 사운드

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
    private bool isSlotMachinePaused = false; // 슬롯 머신으로 인한 일시정지 상태

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
        
        if (slotMachinePanel == null)
        {
            Debug.LogWarning("InGameUIManager: slotMachinePanel이 Inspector에서 할당되지 않았습니다!");
        }
        
        if (slotMachineMgr == null)
        {
            slotMachineMgr = FindObjectOfType<SlotMachineMgr>();
            if (slotMachineMgr == null)
            {
                Debug.LogWarning("InGameUIManager: SlotMachineMgr를 찾을 수 없습니다!");
            }
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
        
        // 버튼 리스너 설정
        SetupButtonListeners();
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
        // 필수 초기화 코드 추가
        InitializeUI();
        
        // 버튼 리스너 설정 (Awake에서도 호출하지만 안전을 위해 중복 호출)
        SetupButtonListeners();
        
        RegisterAllEnemies();
        CheckEventSystem();
        
        // 디버그 로그 추가
        Debug.Log("InGameUIManager Start 완료");
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
        
        // 슬롯 머신 패널 초기 상태 설정
        if (slotMachinePanel != null)
        {
            slotMachinePanel.SetActive(false);
        }
    }

    // 버튼 리스너 설정 메서드 개선
    private void SetupButtonListeners()
    {
        // 일시정지 버튼 리스너 설정
        if (pauseButton != null)
        {
            // 기존 리스너 제거 후 새로 추가 (중복 방지)
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
            Debug.Log("일시정지 버튼 리스너 설정 완료");
        }
        else
        {
            Debug.LogError("일시정지 버튼 참조가 없습니다!");
            // 버튼 참조 찾기 시도
            pauseButton = GameObject.Find("PauseButton")?.GetComponent<Button>();
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(TogglePause);
                Debug.Log("일시정지 버튼 참조 찾기 및 리스너 설정 완료");
            }
        }
        
        // 게임 재개 버튼 리스너 설정
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log("게임 재개 버튼 리스너 설정 완료");
        }
    }

    private void CheckEventSystem()
    {
        // EventSystem 확인
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogError("EventSystem이 씬에 없습니다!");
        }
    }

    // 일시정지 토글 메서드 개선
    public void TogglePause()
    {
        // 슬롯 머신으로 인한 일시정지 상태에서는 무시
        if (isSlotMachinePaused)
        {
            Debug.Log("슬롯 머신 패널이 활성화된 상태에서는 일시정지를 토글할 수 없습니다.");
            return;
        }
        
        isPaused = !isPaused;
        IsGamePaused = isPaused; // 정적 변수 업데이트
        
        // 일시정지 패널 토글
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        
        // 타임스케일 설정
        Time.timeScale = isPaused ? 0f : 1f;
        
        Debug.Log($"게임 일시정지 상태 변경: {isPaused}, Time.timeScale: {Time.timeScale}");
        
        // 디버그용 로그 추가
        if (pausePanel != null)
        {
            Debug.Log($"일시정지 패널 활성화 상태: {pausePanel.activeSelf}");
        }
    }

    /// <summary>
    /// 경험치 추가
    /// </summary>
    public void AddExperience(int amount)
    {
        currentExp += amount;
        
        // 경험치 바 업데이트
        if (expBar != null)
        {
            expBar.value = currentExp;
        }
        
        Debug.Log($"경험치 획득: +{amount}, 현재 경험치: {currentExp}/{maxExp}");
        
        // 레벨업 체크
        if (currentExp >= maxExp)
        {
            LevelUp();
        }
        
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
        
        // 경험치 초과분 이월
        currentExp = currentExp - maxExp;
        if (currentExp < 0) currentExp = 0;
        
        // 경험치 바 업데이트
        if (expBar != null)
        {
            expBar.maxValue = maxExp;
            expBar.value = currentExp;
        }
        
        // 레벨 텍스트 업데이트
        if (levelText != null)
        {
            levelText.text = $"Lv {playerLevel}";
        }
        
        Debug.Log($"레벨 업! 현재 레벨: {playerLevel}, 다음 레벨 경험치: {currentExp}/{maxExp}");
        
        // 데이터 저장
        SavePersistentData();
        
        // 레벨업 사운드 재생
        if (levelUpSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(levelUpSound);
            }
        }
        
        // 슬롯 머신 패널 표시 및 게임 일시정지
        ShowSlotMachinePanel();
        
        // 여기에 레벨업 보상 로직 추가 (스탯 증가 등)
    }
    
    /// <summary>
    /// 슬롯 머신 패널을 표시하고 게임을 일시정지합니다.
    /// </summary>
    private void ShowSlotMachinePanel()
    {
        if (slotMachinePanel != null)
        {
            // 슬롯 머신 패널 활성화
            slotMachinePanel.SetActive(true);
            
            // 슬롯 머신 초기화
            if (slotMachineMgr != null)
            {
                slotMachineMgr.InitializeSlotMachine();
            }
            
            // 레벨업 사운드 재생
            if (levelUpSound != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(levelUpSound);
                }
            }
            
            // 게임 일시정지
            isSlotMachinePaused = true;
            Time.timeScale = 0f;
            
            Debug.Log("슬롯 머신 패널 표시 및 게임 일시정지");
        }
    }
    
    /// <summary>
    /// 슬롯 머신 패널을 닫고 게임을 재개합니다.
    /// </summary>
    public void CloseSlotMachinePanel()
    {
        if (slotMachinePanel != null)
        {
            // 슬롯 머신 패널 비활성화
            slotMachinePanel.SetActive(false);
            
            // 게임 재개
            isSlotMachinePaused = false;
            
            // 일시정지 상태가 아니면 게임 시간 정상화
            if (!isPaused)
            {
                Time.timeScale = 1f;
            }
            
            Debug.Log("슬롯 머신 패널 닫기 및 게임 재개");
        }
    }
    
    /// <summary>
    /// 선택한 스킬을 적용합니다.
    /// </summary>
    /// <param name="skillIndex">선택한 스킬의 인덱스</param>
    public void ApplySelectedSkill(int skillIndex)
    {
        Debug.Log($"선택한 스킬 적용: {skillIndex}");
        
        string statName = "";
        string statValue = "";
        
        switch (skillIndex)
        {
            case 0: // 공격력 증가
                if (playerManager != null)
                {
                    playerManager.IncreaseAttack(5);
                    statName = "공격력";
                    statValue = "+5";
                    Debug.Log("공격력 +5 증가");
                }
                break;
            case 1: // 방어력 증가
                if (playerManager != null)
                {
                    playerManager.IncreaseDefense(3);
                    statName = "방어력";
                    statValue = "+3";
                    Debug.Log("방어력 +3 증가");
                }
                break;
            case 2: // 최대 체력 증가
                if (playerManager != null)
                {
                    playerManager.IncreaseMaxHealth(20);
                    statName = "최대 체력";
                    statValue = "+20";
                    Debug.Log("최대 체력 +20 증가");
                }
                break;
            case 3: // 이동 속도 증가
                if (playerManager != null)
                {
                    playerManager.IncreaseMoveSpeed(0.2f);
                    statName = "이동 속도";
                    statValue = "+0.2";
                    Debug.Log("이동 속도 +0.2 증가");
                }
                break;
            case 4: // 크리티컬 확률 증가
                if (playerManager != null)
                {
                    playerManager.IncreaseCriticalChance(0.05f);
                    statName = "크리티컬 확률";
                    statValue = "+5%";
                    Debug.Log("크리티컬 확률 +5% 증가");
                }
                break;
            default:
                Debug.LogWarning($"알 수 없는 스킬 인덱스: {skillIndex}");
                break;
        }
        
        // 능력치 증가 알림 표시 (선택 사항)
        ShowStatIncreaseNotification(statName, statValue);
        
        // 슬롯 머신 패널 닫기
        CloseSlotMachinePanel();
    }

    /// <summary>
    /// 능력치 증가 알림을 표시합니다.
    /// </summary>
    private void ShowStatIncreaseNotification(string statName, string statValue)
    {
        if (string.IsNullOrEmpty(statName) || string.IsNullOrEmpty(statValue))
            return;
        
        // 여기에 알림 UI를 표시하는 코드를 추가할 수 있습니다.
        // 예: 화면 상단에 잠시 표시되는 텍스트 등
        
        Debug.Log($"{statName} {statValue} 증가!");
        
        // 예시: 간단한 코루틴으로 임시 텍스트 표시
        // StartCoroutine(ShowTemporaryText($"{statName} {statValue} 증가!"));
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
        
        // 경험치 보상 (20%로 고정)
        int expReward = Mathf.RoundToInt(maxExp * 0.2f);
        AddExperience(expReward);
        
        // 골드 보상 (고정 150골드)
        AddGold(150);
    }

    // 모든 적 등록 메서드 - 중복 정의된 메서드를 하나로 통합
    private void RegisterAllEnemies()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        Debug.Log($"InGameUIManager: {enemies.Length}개의 적 발견, 이벤트 구독 시작");
        
        foreach (var enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
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

    // 게임 재개 메서드 개선
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
            Debug.Log("게임 재개 버튼으로 게임 재개");
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

    // 디버그용 메서드 추가
    [ContextMenu("일시정지 토글 테스트")]
    private void TestTogglePause()
    {
        TogglePause();
    }
    
    // 디버그용 메서드 추가
    [ContextMenu("레벨업 테스트")]
    private void TestLevelUp()
    {
        LevelUp();
    }
    
    // 디버그용 메서드 추가
    [ContextMenu("슬롯 머신 테스트")]
    private void TestSlotMachine()
    {
        ShowSlotMachinePanel();
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
        
        // 슬롯 머신 패널이 활성화된 상태에서는 ESC 키로 일시정지 토글 무시
        if (isSlotMachinePaused)
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
                // 버튼 리스너 설정
                pauseButton.onClick.RemoveAllListeners();
                pauseButton.onClick.AddListener(TogglePause);
                Debug.Log("OnEnable에서 일시정지 버튼 참조 찾음 및 리스너 설정 완료");
            }
        }
    }
} 