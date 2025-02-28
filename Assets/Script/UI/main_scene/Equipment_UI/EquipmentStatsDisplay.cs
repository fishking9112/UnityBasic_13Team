using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class EquipmentStatsDisplay : MonoBehaviour
{
    [System.Serializable]
    public class TotalStats
    {
        public int 최대체력;
        public int 공격력;
        public int 방어력;
        public float 방어율;
        public float 크리티컬율;
        public float 이동속도;
    }

    [System.Serializable]
    public class WeaponInventoryData
    {
        public List<WeaponItem> items;
    }

    [System.Serializable]
    public class WeaponItem
    {
        public int id;
        public int Equip;
    }

    [System.Serializable]
    public class WeaponDataList
    {
        public List<WeaponData> items;
    }

    [System.Serializable]
    public class WeaponData
    {
        public int id;
        public string name;
        public int capability_value;
        public int Type;
        public int rating;
    }

    [Header("스탯 UI")]
    [SerializeField] private Text attackStatText;
    [SerializeField] private Text defenseStatText;

    [Header("장비 슬롯")]
    [SerializeField] private Image weaponSlotImage;
    [SerializeField] private Image armorSlotImage;
    [SerializeField] private Image accessorySlotImage;

    // 데이터 저장
    private TotalStats totalStats;
    private List<WeaponItem> inventoryItems = new List<WeaponItem>();
    private Dictionary<int, WeaponData> weaponDataDict = new Dictionary<int, WeaponData>();
    
    // 장착된 아이템 참조
    private WeaponData equippedWeapon;
    private WeaponData equippedArmor;
    private WeaponData equippedAccessory;

    private void Start()
    {
        // 무기 데이터 로드
        LoadWeaponData();
        
        // 인벤토리 데이터 로드
        LoadInventoryData();
        
        // 스탯 데이터 로드
        LoadTotalStats();
        
        // UI 업데이트
        UpdateUI();
    }

    private void LoadWeaponData()
    {
        string filePath = Path.Combine(Application.dataPath, "Script/Chart/Weapon_data.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            WeaponDataList weaponDataList = JsonUtility.FromJson<WeaponDataList>(jsonData);
            
            // 사전에 무기 데이터 저장
            foreach (WeaponData weapon in weaponDataList.items)
            {
                weaponDataDict[weapon.id] = weapon;
            }
            
            Debug.Log($"무기 데이터 {weaponDataDict.Count}개 로드 완료");
        }
        else
        {
            Debug.LogError("Weapon_data.json 파일을 찾을 수 없습니다. 경로: " + filePath);
        }
    }

    private void LoadInventoryData()
    {
        string inventoryPath = Path.Combine(Application.persistentDataPath, "WeaponInventory.json");
        if (File.Exists(inventoryPath))
        {
            try
            {
                string jsonData = File.ReadAllText(inventoryPath);
                
                // JSON 배열을 WeaponInventoryData 형식으로 변환
                inventoryItems = JsonUtility.FromJson<WeaponInventoryData>("{\"items\":" + jsonData + "}").items;
                
                Debug.Log($"인벤토리 아이템 {inventoryItems.Count}개 로드 완료");
                
                // 장착된 아이템 찾기
                FindEquippedItems();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"인벤토리 데이터 로드 중 오류: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("WeaponInventory.json 파일을 찾을 수 없습니다.");
        }
    }

    private void LoadTotalStats()
    {
        string statsPath = Path.Combine(Application.dataPath, "Script/Chart/TotalStats.json");
        if (File.Exists(statsPath))
        {
            string jsonData = File.ReadAllText(statsPath);
            totalStats = JsonUtility.FromJson<TotalStats>(jsonData);
            
            Debug.Log("TotalStats.json 로드 완료");
        }
        else
        {
            Debug.LogError("TotalStats.json 파일을 찾을 수 없습니다. 경로: " + statsPath);
            
            // 기본값 설정
            totalStats = new TotalStats
            {
                최대체력 = 100,
                공격력 = 10,
                방어력 = 5,
                방어율 = 0.05f,
                크리티컬율 = 0.1f,
                이동속도 = 3.0f
            };
        }
    }

    private void FindEquippedItems()
    {
        equippedWeapon = null;
        equippedArmor = null;
        equippedAccessory = null;
        
        foreach (WeaponItem item in inventoryItems)
        {
            // 장착된 아이템만 처리
            if (item.Equip == 1 && weaponDataDict.TryGetValue(item.id, out WeaponData weaponData))
            {
                // 아이템 타입에 따라 분류
                switch (weaponData.Type)
                {
                    case 1: // 무기
                        equippedWeapon = weaponData;
                        Debug.Log($"장착된 무기 발견: {weaponData.name} (ID: {weaponData.id})");
                        break;
                    case 2: // 방어구
                        equippedArmor = weaponData;
                        Debug.Log($"장착된 방어구 발견: {weaponData.name} (ID: {weaponData.id})");
                        break;
                    case 3: // 장신구
                        equippedAccessory = weaponData;
                        Debug.Log($"장착된 장신구 발견: {weaponData.name} (ID: {weaponData.id})");
                        break;
                }
            }
        }
    }

    private void UpdateUI()
    {
        // 공격력과 방어력만 업데이트
        if (totalStats != null)
        {
            if (attackStatText != null) 
            {
                attackStatText.text = $"공격력: {totalStats.공격력}";
                attackStatText.color = new Color(1f, 0.6f, 0.2f); // 주황색 (공격력)
            }
            
            if (defenseStatText != null) 
            {
                defenseStatText.text = $"방어력: {totalStats.방어력}";
                defenseStatText.color = new Color(0.2f, 0.6f, 1f); // 파란색 (방어력)
            }
        }
        
        // 장착 아이템 이미지만 업데이트
        UpdateEquipmentSlotImage(weaponSlotImage, equippedWeapon);
        UpdateEquipmentSlotImage(armorSlotImage, equippedArmor);
        UpdateEquipmentSlotImage(accessorySlotImage, equippedAccessory);
    }

    private void UpdateEquipmentSlotImage(Image slotImage, WeaponData equippedItem)
    {
        if (slotImage == null) return;
        
        if (equippedItem != null)
        {
            // 아이템 이미지 설정
            string spriteName = equippedItem.id.ToString();
            bool spriteFound = false;
            
            // 1. 먼저 Resources/item/ 폴더에서 찾기
            Sprite itemSprite = Resources.Load<Sprite>("item/" + spriteName);
            
            if (itemSprite != null)
            {
                slotImage.sprite = itemSprite;
                slotImage.color = Color.white;
                spriteFound = true;
                Debug.Log($"아이템 이미지 로드 성공 (경로1): {spriteName}");
            }
            else
            {
                // 2. 타입별 폴더에서 찾기 시도
                string typeFolder = "";
                switch (equippedItem.Type)
                {
                    case 1: typeFolder = "weapons"; break;
                    case 2: typeFolder = "armors"; break;
                    case 3: typeFolder = "accessories"; break;
                }
                
                if (!string.IsNullOrEmpty(typeFolder))
                {
                    itemSprite = Resources.Load<Sprite>($"item/{typeFolder}/{spriteName}");
                    if (itemSprite != null)
                    {
                        slotImage.sprite = itemSprite;
                        slotImage.color = Color.white;
                        spriteFound = true;
                        Debug.Log($"아이템 이미지 로드 성공 (경로2): item/{typeFolder}/{spriteName}");
                    }
                }
            }
            
            // 3. 위 방법으로 찾지 못한 경우 모든 스프라이트 검색
            if (!spriteFound)
            {
                // 모든 item 폴더의 스프라이트 로드
                Sprite[] allSprites = Resources.LoadAll<Sprite>("item");
                Debug.Log($"Resources/item 폴더에서 {allSprites.Length}개의 스프라이트 로드됨");
                
                foreach (Sprite sprite in allSprites)
                {
                    if (sprite.name == spriteName)
                    {
                        slotImage.sprite = sprite;
                        slotImage.color = Color.white;
                        spriteFound = true;
                        Debug.Log($"아이템 이미지 로드 성공 (이름 검색): {spriteName}");
                        break;
                    }
                }
                
                // 4. 타입별 이름 패턴으로 검색 (예: "weapon_1", "armor_2" 등)
                if (!spriteFound)
                {
                    string typePrefix = "";
                    switch (equippedItem.Type)
                    {
                        case 1: typePrefix = "weapon_"; break;
                        case 2: typePrefix = "armor_"; break;
                        case 3: typePrefix = "accessory_"; break;
                    }
                    
                    string alternativeName = typePrefix + spriteName;
                    
                    foreach (Sprite sprite in allSprites)
                    {
                        if (sprite.name == alternativeName)
                        {
                            slotImage.sprite = sprite;
                            slotImage.color = Color.white;
                            spriteFound = true;
                            Debug.Log($"아이템 이미지 로드 성공 (대체 이름): {alternativeName}");
                            break;
                        }
                    }
                }
            }
            
            // 이미지를 찾지 못한 경우 기본 이미지 또는 반투명 처리
            if (!spriteFound)
            {
                Debug.LogWarning($"아이템 이미지를 찾을 수 없습니다: ID {equippedItem.id}, 타입 {equippedItem.Type}, 이름 {equippedItem.name}");
                
                // 타입별 기본 이미지 적용
                string defaultImagePath = "";
                switch (equippedItem.Type)
                {
                    case 1: defaultImagePath = "item/default_weapon"; break;
                    case 2: defaultImagePath = "item/default_armor"; break;
                    case 3: defaultImagePath = "item/default_accessory"; break;
                }
                
                Sprite defaultSprite = Resources.Load<Sprite>(defaultImagePath);
                if (defaultSprite != null)
                {
                    slotImage.sprite = defaultSprite;
                    slotImage.color = new Color(1, 1, 1, 0.7f); // 약간 반투명
                }
                else
                {
                    // 기본 이미지도 없는 경우
                    slotImage.color = new Color(0.8f, 0.8f, 0.8f, 0.5f); // 회색 반투명
                }
            }
            
            // Outline 컴포넌트가 있는지 확인 후 등급에 따른 테두리 색상 설정
            Outline outline = slotImage.GetComponent<Outline>();
            if (outline != null)
            {
                switch (equippedItem.rating)
                {
                    case 1: // 일반
                        outline.effectColor = Color.white;
                        break;
                    case 2: // 고급
                        outline.effectColor = Color.green;
                        break;
                    case 3: // 희귀
                        outline.effectColor = Color.blue;
                        break;
                    case 4: // 전설
                        outline.effectColor = new Color(1f, 0.5f, 0f); // 주황색
                        break;
                }
            }
        }
        else
        {
            // 장착된 아이템이 없을 경우
            slotImage.sprite = null;
            slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 회색 반투명
        }
    }

    // 데이터 새로고침 (외부에서 호출 가능)
    public void RefreshData()
    {
        Debug.Log("EquipmentStatsDisplay: 데이터 새로고침 시작");
        LoadInventoryData();
        LoadTotalStats();
        UpdateUI();
        Debug.Log("EquipmentStatsDisplay: 데이터 새로고침 완료");
    }

    // 인벤토리 변경 이벤트 구독을 위한 OnEnable/OnDisable
    private void OnEnable()
    {
        // 인벤토리 매니저 이벤트 구독
        EquipmentInventoryManager inventoryManager = FindObjectOfType<EquipmentInventoryManager>();
        if (inventoryManager != null)
        {
            // 이벤트 구독
            inventoryManager.OnInventoryChanged += RefreshData;
            Debug.Log("EquipmentStatsDisplay: 인벤토리 변경 이벤트 구독 완료");
        }
        else
        {
            Debug.LogWarning("EquipmentStatsDisplay: 인벤토리 매니저를 찾을 수 없습니다.");
        }
        
        // 초기 데이터 로드
        RefreshData();
    }

    private void OnDisable()
    {
        // 인벤토리 매니저 이벤트 구독 해제
        EquipmentInventoryManager inventoryManager = FindObjectOfType<EquipmentInventoryManager>();
        if (inventoryManager != null)
        {
            // 이벤트 구독 해제
            inventoryManager.OnInventoryChanged -= RefreshData;
            Debug.Log("EquipmentStatsDisplay: 인벤토리 변경 이벤트 구독 해제");
        }
    }
} 