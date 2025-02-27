using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BackendData.GameData;
using BackendData.GameData.WeaponInventory;

public class GachaManager : MonoBehaviour
{
    [Header("뽑기 설정")]
    [Tooltip("1회 뽑기 비용")]
    public int singleGachaPrice = 300;
    
    [Tooltip("10회 뽑기 비용")]
    public int tenGachaPrice = 2900;
    
    [Tooltip("희귀 등급 이상 확률 1.5배 적용")]
    [SerializeField] private bool increasedRateEnabled = false;
    
    [Tooltip("확률 증가 배율 (기본 1.5배)")]
    [Range(1.0f, 3.0f)]
    [SerializeField] private float rateMultiplier = 1.5f;
    
    [Header("UI 요소")]
    [SerializeField] private Button singleGachaButton;
    [SerializeField] private Button tenGachaButton;
    [SerializeField] private Toggle increasedRateToggle;
    
    // 결과창 프리팹 참조
    [SerializeField] private GameObject resultPanelPrefab;
    private GachaResultPanel resultPanel;
    
    // 차트 데이터
    private Dictionary<string, int> rarityData;
    private List<WeaponData> weaponData;
    
    // 유저 데이터
    private UserData userData;
    private BackendData.GameData.WeaponInventory.Manager weaponInventory;
    
    // 등급별 확률 (%)
    private int normalProb;
    private int rareProb;
    private int epicProb;
    private int legendaryProb;
    
    // 최근 획득한 아이템 목록
    private List<WeaponData> obtainedItems = new List<WeaponData>();
    
    [Header("인벤토리 연동")]
    [SerializeField] private EquipmentInventoryManager equipmentInventoryManager;
    
    void Start()
    {
        // 버튼 이벤트 연결
        singleGachaButton.onClick.AddListener(() => DoGacha(1));
        tenGachaButton.onClick.AddListener(() => DoGacha(10));
        
        // 확률 증가 토글 이벤트 연결
        if (increasedRateToggle != null)
        {
            increasedRateToggle.isOn = increasedRateEnabled;
            increasedRateToggle.onValueChanged.AddListener(OnIncreasedRateToggleChanged);
        }
        
        // 데이터 로드
        LoadData();
    }
    
    private void OnIncreasedRateToggleChanged(bool isOn)
    {
        increasedRateEnabled = isOn;
        Debug.Log($"확률 증가 모드: {(increasedRateEnabled ? "활성화" : "비활성화")}");
    }
    
    private void LoadData()
    {
        try
        {
            // 희귀도 데이터 로드
            string rarityPath = Path.Combine(Application.dataPath, "Script/Chart/Rarity_data.json");
            if (File.Exists(rarityPath))
            {
                string rarityJson = File.ReadAllText(rarityPath);
                
                // 기본값 설정
                normalProb = 60;
                rareProb = 30;
                epicProb = 9;
                legendaryProb = 1;
                
                // JSON 파싱 시도
                try {
                    // RarityDataWrapper 클래스를 사용하여 파싱
                    var wrapper = JsonUtility.FromJson<RarityDataWrapper>(rarityJson);
                    if (wrapper != null && wrapper.rarities != null)
                    {
                        rarityData = new Dictionary<string, int>();
                        foreach (var rarity in wrapper.rarities)
                        {
                            rarityData[rarity.name] = rarity.probability;
                            
                            // 확률 설정
                            if (rarity.name == "normal") normalProb = rarity.probability;
                            if (rarity.name == "rare") rareProb = rarity.probability;
                            if (rarity.name == "epic") epicProb = rarity.probability;
                            if (rarity.name == "legendary") legendaryProb = rarity.probability;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogWarning($"희귀도 데이터 파싱 실패: {ex.Message}. 기본값을 사용합니다.");
                }
            }
            
            // 무기 데이터 로드
            string weaponPath = Path.Combine(Application.dataPath, "Script/Chart/Weapon_data.json");
            if (File.Exists(weaponPath))
            {
                string weaponJson = File.ReadAllText(weaponPath);
                var wrapper = JsonUtility.FromJson<WeaponDataWrapper>(weaponJson);
                weaponData = wrapper.items;
            }
            else
            {
                // 테스트용 데이터 생성
                weaponData = new List<WeaponData>();
                // 테스트 데이터 추가...
            }
            
            // 유저 데이터 로드
            userData = GameDataManager.Instance.UserData;
            weaponInventory = GameDataManager.Instance.WeaponInventory;
            
            Debug.Log("가챠 데이터 로드 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 중 오류: {ex.Message}");
        }
    }
    
    public void DoGacha(int count)
    {
        try
        {
            // 1. 골드 확인
            int price = count == 1 ? singleGachaPrice : tenGachaPrice;
            if (userData.Gold < price)
            {
                Debug.LogWarning("골드가 부족합니다.");
                return;
            }
            
            // 2. 골드 차감
            GameDataManager.Instance.ReduceGold(price);
            Debug.Log($"골드 차감: {price}, 남은 골드: {userData.Gold}");
            
            // 3. 아이템 뽑기
            obtainedItems.Clear();
            for (int i = 0; i < count; i++)
            {
                // 등급 결정
                int rating = DetermineRating();
                
                // 해당 등급의 아이템 필터링
                var itemsOfRating = weaponData.FindAll(item => item.rating == rating);
                
                if (itemsOfRating.Count > 0)
                {
                    // 랜덤 아이템 선택
                    int randomIndex = UnityEngine.Random.Range(0, itemsOfRating.Count);
                    WeaponData selectedItem = itemsOfRating[randomIndex];
                    
                    // 결과 저장
                    obtainedItems.Add(selectedItem);
                    
                    // 인벤토리에 추가
                    weaponInventory.AddWeapon(selectedItem.id);
                }
            }
            
            // 5. 결과 UI 표시
            if (obtainedItems.Count > 0)
            {
                DisplayGachaResults(count, obtainedItems);
                Debug.Log($"{count}회 뽑기 결과 표시: {obtainedItems.Count}개 아이템");
            }
            else
            {
                Debug.LogError("뽑기 결과가 없습니다.");
            }
            
            // 6. 데이터 저장
            try
            {
                // 변경 사항 저장
                weaponInventory.SaveToJson();
                userData.SaveToJson();
                
                // 인벤토리 UI 업데이트
                if (equipmentInventoryManager != null)
                {
                    equipmentInventoryManager.RefreshInventory();
                }
                else
                {
                    // 인벤토리 매니저 찾기 시도
                    equipmentInventoryManager = FindObjectOfType<EquipmentInventoryManager>();
                    if (equipmentInventoryManager != null)
                    {
                        equipmentInventoryManager.RefreshInventory();
                    }
                    else
                    {
                        Debug.LogWarning("EquipmentInventoryManager를 찾을 수 없습니다. 인벤토리가 업데이트되지 않을 수 있습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"데이터 저장 중 오류: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"뽑기 중 오류: {ex.Message}");
        }
    }
    
    private int DetermineRating()
    {
        // 1~100 사이의 랜덤 값 생성
        int rand = UnityEngine.Random.Range(1, 101);
        
        // 확률 계산
        float legendaryProbability = legendaryProb;
        float epicProbability = epicProb;
        float rareProbability = rareProb;
        float normalProbability = normalProb;
        
        // 확률 증가 모드가 활성화된 경우
        if (increasedRateEnabled)
        {
            // 희귀 등급 이상의 확률만 증가 (일반 등급은 감소)
            legendaryProbability *= rateMultiplier;
            epicProbability *= rateMultiplier;
            rareProbability *= rateMultiplier;
            
            // 전체 확률이 100%를 넘지 않도록 조정
            float totalProbability = legendaryProbability + epicProbability + rareProbability + normalProbability;
            if (totalProbability > 100)
            {
                // 일반 등급 확률 감소
                normalProbability = 100 - (legendaryProbability + epicProbability + rareProbability);
                if (normalProbability < 0) normalProbability = 0;
            }
            
            Debug.Log($"확률 증가 적용: 전설({legendaryProbability}%), 에픽({epicProbability}%), 희귀({rareProbability}%), 일반({normalProbability}%)");
        }
        
        // 확률에 따라 등급 결정
        if (rand <= legendaryProbability)
        {
            return 4; // 전설
        }
        else if (rand <= legendaryProbability + epicProbability)
        {
            return 3; // 에픽
        }
        else if (rand <= legendaryProbability + epicProbability + rareProbability)
        {
            return 2; // 희귀
        }
        else
        {
            return 1; // 일반
        }
    }
    
    private void DisplayGachaResults(int count, List<WeaponData> items)
    {
        // 결과 패널이 없으면 생성
        if (resultPanel == null)
        {
            // 정확한 경로로 Content 찾기
            Transform contentTransform = GameObject.Find("Canvas/Scroll View/Viewport/MainUI/Shop_Scroll_View/Viewport/Content")?.transform;
            
            if (contentTransform == null)
            {
                Debug.LogWarning("정확한 경로로 Content를 찾지 못했습니다. 다른 방법으로 시도합니다.");
                
                // 태그로 찾기
                GameObject contentObj = GameObject.FindWithTag("ShopContent");
                if (contentObj != null)
                {
                    contentTransform = contentObj.transform;
                }
                else
                {
                    // 이름으로 찾기
                    contentObj = GameObject.Find("Content");
                    if (contentObj != null)
                    {
                        contentTransform = contentObj.transform;
                    }
                    else
                    {
                        // Canvas 사용
                        Canvas canvas = FindObjectOfType<Canvas>();
                        if (canvas != null)
                        {
                            contentTransform = canvas.transform;
                            Debug.LogWarning("Content를 찾지 못해 Canvas를 사용합니다.");
                        }
                        else
                        {
                            Debug.LogError("UI 부모 요소를 찾을 수 없습니다.");
                            return;
                        }
                    }
                }
            }
            
            // Content 아래에 패널 생성
            GameObject panelObj = Instantiate(resultPanelPrefab, contentTransform);
            resultPanel = panelObj.GetComponent<GachaResultPanel>();
            
            if (resultPanel == null)
            {
                Debug.LogError("결과 패널 프리팹에 GachaResultPanel 컴포넌트가 없습니다.");
                return;
            }
            
            Debug.Log($"결과 패널 생성 완료: {panelObj.name}, 부모: {panelObj.transform.parent.name}");
            
            // 패널 위치 및 크기 설정
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // 전체 화면을 차지하도록 설정
                panelRect.anchorMin = new Vector2(0, 0);
                panelRect.anchorMax = new Vector2(1, 1);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                // 최상위로 가져오기
                panelObj.transform.SetAsLastSibling();
            }
        }
        
        // 패널 활성화 및 초기화
        resultPanel.gameObject.SetActive(true);
        resultPanel.Initialize(items, () => {
            Debug.Log("결과 패널 닫힘");
        });
    }
    
    [Serializable]
    public class WeaponData
    {
        public int id;
        public string name;
        public float capability_value;
        public int Type;
        public int rating;
    }
    
    [Serializable]
    public class WeaponDataWrapper
    {
        public List<WeaponData> items;
    }
    
    [Serializable]
    public class RarityData
    {
        public string name;
        public int probability;
    }
    
    [Serializable]
    public class RarityDataWrapper
    {
        public List<RarityData> rarities;
    }
} 