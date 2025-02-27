using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Canvas canvas;
    
    private Camera mainCamera;
    private InGamePlayerManager playerManager;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (canvas != null)
        {
            canvas.worldCamera = mainCamera;
        }
        else
        {
            Debug.LogError("PlayerHealthBar: Canvas 참조가 없습니다.");
        }
        
        // 체력바 이미지 확인
        if (healthBarFill == null)
        {
            healthBarFill = GetComponentInChildren<Image>();
            Debug.Log("PlayerHealthBar: 자동으로 Image 컴포넌트를 찾았습니다.");
        }
    }
    
    private void Start()
    {
        // 플레이어 매니저 참조 가져오기 - 씬에 있는 인스턴스 직접 찾기
        playerManager = FindObjectOfType<InGamePlayerManager>();
        
        if (playerManager == null)
        {
            playerManager = InGamePlayerManager.Instance;
        }
        
        if (playerManager != null)
        {
            // 체력 변경 이벤트 구독
            playerManager.OnHealthChanged += UpdateHealthBar;
            
            // 초기 체력 설정
            UpdateHealthBar(playerManager.CurrentHealth, playerManager.MaxHealth);
            
            Debug.Log($"PlayerHealthBar: 이벤트 구독 완료, 초기 체력 {playerManager.CurrentHealth}/{playerManager.MaxHealth}");
        }
        else
        {
            Debug.LogError("PlayerHealthBar: InGamePlayerManager 인스턴스를 찾을 수 없습니다.");
        }
    }
    
    private void OnEnable()
    {
        // 활성화될 때 이벤트 재구독 (씬 전환 등의 상황 대비)
        if (playerManager != null)
        {
            playerManager.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerManager.CurrentHealth, playerManager.MaxHealth);
            Debug.Log("PlayerHealthBar: OnEnable에서 이벤트 재구독");
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (playerManager != null)
        {
            playerManager.OnHealthChanged -= UpdateHealthBar;
        }
    }
    
    private void OnDisable()
    {
        // 비활성화될 때 이벤트 구독 해제
        if (playerManager != null)
        {
            playerManager.OnHealthChanged -= UpdateHealthBar;
        }
    }
    
    private void LateUpdate()
    {
        // 항상 카메라를 향하도록 회전 (플레이어 방향과 무관하게)
        if (mainCamera != null)
        {
            // 카메라의 회전만 따라가도록 설정
            transform.rotation = mainCamera.transform.rotation;
        }
        
        // 디버그: 현재 체력바 상태 확인
        if (playerManager != null && healthBarFill != null)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.up * 2f, Color.red);
            // 매 프레임마다 체력바 강제 업데이트 (테스트용)
            float fillAmount = (float)playerManager.CurrentHealth / playerManager.MaxHealth;
            if (Mathf.Abs(healthBarFill.fillAmount - fillAmount) > 0.01f)
            {
                Debug.Log($"체력바 불일치 감지: UI={healthBarFill.fillAmount}, 실제={fillAmount}");
                healthBarFill.fillAmount = fillAmount;
            }
        }
    }
    
    /// <summary>
    /// 체력바 업데이트
    /// </summary>
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBarFill != null)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            
            // 이전 값과 현재 값 비교
            float previousFill = healthBarFill.fillAmount;
            healthBarFill.fillAmount = fillAmount;
            
            Debug.Log($"PlayerHealthBar: 체력바 업데이트 - {currentHealth}/{maxHealth} = {fillAmount} (이전: {previousFill})");
            
            // 변경 사항이 적용되었는지 확인
            if (Mathf.Abs(previousFill - fillAmount) > 0.01f)
            {
                // 강제로 Canvas 업데이트
                if (canvas != null)
                {
                    canvas.enabled = false;
                    canvas.enabled = true;
                }
            }
        }
        else
        {
            Debug.LogError("PlayerHealthBar: healthBarFill이 null입니다.");
        }
    }
    
    // 디버그용 메서드
    [ContextMenu("체력바 테스트")]
    private void TestHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Random.Range(0f, 1f);
            Debug.Log($"체력바 테스트: fillAmount = {healthBarFill.fillAmount}");
        }
    }
}