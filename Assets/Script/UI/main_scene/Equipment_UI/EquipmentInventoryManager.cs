using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class EquipmentInventoryManager : MonoBehaviour
{
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

    [Header("UI References")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int itemsPerRow = 4;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private Vector2 itemSize = new Vector2(100f, 120f);
    [SerializeField] private EquipmentDetailPanel detailPanel;

    private List<WeaponItem> inventoryItems = new List<WeaponItem>();
    private Dictionary<int, WeaponData> weaponDataDict = new Dictionary<int, WeaponData>();
    private GameDataManager gameDataManager;

    private void Start()
    {
        // GameDataManager 참조 가져오기
        gameDataManager = GameDataManager.Instance;
        if (gameDataManager == null)
        {
            Debug.LogError("GameDataManager를 찾을 수 없습니다.");
        }
        
        LoadWeaponData();
        LoadInventoryData();
        GenerateInventoryUI();
        
        // 파일 권한 확인
        CheckFilePermissions();
    }

    private void LoadWeaponData()
    {
        // 직접 파일 경로로 접근
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
        }
        else
        {
            Debug.LogError("Weapon_data.json 파일을 찾을 수 없습니다. 경로: " + filePath);
        }
    }

    private void LoadInventoryData()
    {
        // 테스트용 하드코딩된 데이터 대신 실제 파일에서 로드
        string inventoryPath = Path.Combine(Application.persistentDataPath, "WeaponInventory.json");
        if (File.Exists(inventoryPath))
        {
            try
            {
                string jsonData = File.ReadAllText(inventoryPath);
                
                // 파일 내용 로깅 (디버깅용)
                Debug.Log($"로드된 WeaponInventory.json 내용: {jsonData}");
                
                // JSON 배열을 WeaponInventoryData 형식으로 변환
                inventoryItems = JsonUtility.FromJson<WeaponInventoryData>("{\"items\":" + jsonData + "}").items;
                
                // 로드된 아이템 수 로깅
                Debug.Log($"로드된 인벤토리 아이템 수: {inventoryItems.Count}");
                
                // 로드된 데이터 검증
                Debug.Log("=== 로드된 인벤토리 상태 ===");
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    WeaponItem item = inventoryItems[i];
                    Debug.Log($"인덱스 {i}: ID {item.id}, 장착 상태 {item.Equip}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"인벤토리 데이터 로드 중 오류: {ex.Message}");
                // 오류 발생 시 기본 데이터 사용
                UseDefaultInventoryData();
            }
        }
        else
        {
            Debug.LogWarning("WeaponInventory.json 파일을 찾을 수 없습니다. 기본 데이터를 사용합니다.");
            UseDefaultInventoryData();
        }
    }

    private void UseDefaultInventoryData()
    {
        // 파일이 없을 경우 기본 데이터 사용
        string defaultData = "[{\"id\":3003,\"Equip\":0},{\"id\":3003,\"Equip\":0},{\"id\":3002,\"Equip\":0},{\"id\":1003,\"Equip\":0},{\"id\":2002,\"Equip\":0},{\"id\":2003,\"Equip\":0},{\"id\":2002,\"Equip\":0},{\"id\":1002,\"Equip\":0},{\"id\":3002,\"Equip\":0},{\"id\":1002,\"Equip\":0},{\"id\":3003,\"Equip\":0}]";
        inventoryItems = JsonUtility.FromJson<WeaponInventoryData>("{\"items\":" + defaultData + "}").items;
        
        // 기본 데이터를 파일로 저장
        SaveInventoryData();
    }

    private void GenerateInventoryUI()
    {
        // 기존 아이템 UI 제거
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 그리드 레이아웃 설정
        GridLayoutGroup gridLayout = contentPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = contentPanel.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.cellSize = itemSize;
        gridLayout.spacing = new Vector2(itemSpacing, itemSpacing);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = itemsPerRow;
        
        // 디버깅: 인벤토리 아이템 상태 출력
        Debug.Log("=== 인벤토리 아이템 상태 ===");
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            WeaponItem item = inventoryItems[i];
            Debug.Log($"인덱스 {i}: ID {item.id}, 장착 상태 {item.Equip}");
        }
        
        // 인벤토리 아이템 생성
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            WeaponItem item = inventoryItems[i];
            
            if (weaponDataDict.TryGetValue(item.id, out WeaponData weaponData))
            {
                // 아이템 UI 생성
                GameObject itemObj = Instantiate(itemPrefab, contentPanel);
                
                // EquipmentItemUI 컴포넌트 가져오기
                EquipmentItemUI itemUI = itemObj.GetComponent<EquipmentItemUI>();
                if (itemUI != null)
                {
                    // EquipmentItemUI 컴포넌트를 사용하여 아이템 데이터 설정
                    itemUI.SetItemData(item.id, weaponData.name, item.Equip == 1, weaponData.rating);
                    
                    // 디버깅용 인덱스 표시 (안전한 방법으로 수정)
                    try
                    {
                        // 기존 Text 컴포넌트 찾기
                        Text existingText = itemObj.GetComponentInChildren<Text>();
                        
                        if (existingText != null)
                        {
                            // 기존 Text 컴포넌트 사용
                            existingText.text = i.ToString();
                            existingText.color = Color.red;
                            existingText.fontSize = 20;
                        }
                        else
                        {
                            // 새 GameObject 생성하여 Text 추가
                            GameObject textObj = new GameObject("IndexText");
                            textObj.transform.SetParent(itemObj.transform, false);
                            
                            Text indexText = textObj.AddComponent<Text>();
                            indexText.text = i.ToString();
                            indexText.color = Color.red;
                            indexText.fontSize = 20;
                            
                            // 폰트 설정
                            indexText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                            
                            // 위치 및 크기 설정
                            RectTransform rectTransform = indexText.rectTransform;
                            rectTransform.anchoredPosition = new Vector2(10, 10);
                            rectTransform.sizeDelta = new Vector2(30, 30);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"인덱스 텍스트 추가 중 오류: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError("EquipmentItemUI 컴포넌트를 찾을 수 없습니다.");
                }
                
                // 아이템 클릭 이벤트 설정
                Button itemButton = itemObj.GetComponent<Button>();
                if (itemButton != null)
                {
                    int itemIndex = i; // 클로저 문제 방지
                    itemButton.onClick.AddListener(() => OnItemClicked(itemIndex));
                }
            }
            else
            {
                Debug.LogWarning("ID가 " + item.id + "인 아이템 데이터를 찾을 수 없습니다.");
            }
        }
    }

    private void OnItemClicked(int itemIndex)
    {
        // 아이템 클릭 시 처리
        if (itemIndex < 0 || itemIndex >= inventoryItems.Count)
        {
            Debug.LogError($"유효하지 않은 아이템 인덱스: {itemIndex}, 아이템 수: {inventoryItems.Count}");
            return;
        }
        
        WeaponItem item = inventoryItems[itemIndex];
        
        Debug.Log($"아이템 클릭: 인덱스 {itemIndex}, ID {item.id}, 장착 상태 {item.Equip}");
        
        if (weaponDataDict.TryGetValue(item.id, out WeaponData weaponData))
        {
            // 상세 정보 패널 표시
            if (detailPanel != null)
            {
                detailPanel.ShowPanel(
                    item.id, 
                    weaponData.name, 
                    weaponData.Type, 
                    weaponData.rating, 
                    weaponData.capability_value, 
                    item.Equip == 1, 
                    itemIndex
                );
            }
        }
        else
        {
            Debug.LogError($"ID가 {item.id}인 아이템 데이터를 찾을 수 없습니다.");
        }
    }

    private void SaveInventoryData()
    {
        try
        {
            // 저장 전 인벤토리 상태 로깅
            Debug.Log("=== 저장 전 인벤토리 상태 ===");
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                WeaponItem item = inventoryItems[i];
                Debug.Log($"인덱스 {i}: ID {item.id}, 장착 상태 {item.Equip}");
            }
            
            // 파일 경로 설정
            string inventoryPath = Path.Combine(Application.persistentDataPath, "WeaponInventory.json");
            Debug.Log($"저장 경로: {inventoryPath}");
            
            // 간단한 방식으로 JSON 배열 구성
            List<string> jsonItems = new List<string>();
            foreach (WeaponItem item in inventoryItems)
            {
                jsonItems.Add($"{{\"id\":{item.id},\"Equip\":{item.Equip}}}");
            }
            
            string jsonArray = "[" + string.Join(",", jsonItems) + "]";
            Debug.Log($"저장할 JSON: {jsonArray}");
            
            // 파일 저장 (File.WriteAllText 대신 FileStream 사용)
            using (FileStream fs = new FileStream(inventoryPath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(jsonArray);
                writer.Flush();
                fs.Flush(true); // 강제로 디스크에 쓰기
            }
            
            // 저장 확인
            if (File.Exists(inventoryPath))
            {
                string content = File.ReadAllText(inventoryPath);
                Debug.Log($"저장된 파일 내용: {content}");
                
                if (content != jsonArray)
                {
                    Debug.LogWarning("저장된 내용이 원본과 다릅니다!");
                    Debug.LogWarning($"원본: {jsonArray}");
                    Debug.LogWarning($"저장됨: {content}");
                }
            }
            else
            {
                Debug.LogError($"파일이 저장되지 않았습니다: {inventoryPath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"인벤토리 데이터 저장 중 오류: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public bool ToggleEquipItem(int itemIndex, int itemType)
    {
        if (itemIndex < 0 || itemIndex >= inventoryItems.Count)
        {
            Debug.LogError($"유효하지 않은 아이템 인덱스: {itemIndex}, 아이템 수: {inventoryItems.Count}");
            return false;
        }
        
        // 현재 아이템 참조
        WeaponItem targetItem = inventoryItems[itemIndex];
        int oldEquipState = targetItem.Equip;
        
        Debug.Log($"ToggleEquipItem 호출: 인덱스 {itemIndex}, 아이템 ID {targetItem.id}, 현재 상태 {oldEquipState}");
        
        // 장착 상태 토글
        targetItem.Equip = (targetItem.Equip == 1) ? 0 : 1;
        
        // 같은 타입의 다른 아이템 해제
        if (targetItem.Equip == 1)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (i != itemIndex && // 현재 아이템이 아니고
                    weaponDataDict.TryGetValue(inventoryItems[i].id, out WeaponData weaponData) &&
                    weaponData.Type == itemType && // 같은 타입이고
                    inventoryItems[i].Equip == 1) // 장착 중이면
                {
                    // 해제
                    inventoryItems[i].Equip = 0;
                    Debug.Log($"다른 아이템 해제: 인덱스 {i}, ID {inventoryItems[i].id}");
                }
            }
        }
        
        Debug.Log($"장착 상태 변경: 인덱스 {itemIndex}, ID {targetItem.id}, 이전 {oldEquipState}, 현재 {targetItem.Equip}");
        
        // 변경 여부 확인
        bool changed = oldEquipState != targetItem.Equip;
        
        if (changed)
        {
            // 변경 후 인벤토리 상태 확인
            Debug.Log("=== 장착 상태 변경 후 인벤토리 상태 ===");
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                WeaponItem item = inventoryItems[i];
                Debug.Log($"인덱스 {i}: ID {item.id}, 장착 상태 {item.Equip}");
            }
            
            // GameDataManager를 통한 저장 (중요!)
            if (gameDataManager != null)
            {
                Debug.Log($"GameDataManager를 통해 장착 상태 변경 시도: ID {targetItem.id}, 인덱스 {itemIndex}, 상태 {targetItem.Equip}");
                
                try
                {
                    // GameDataManager의 WeaponInventory 데이터 업데이트
                    var weaponInventoryManager = gameDataManager.GetWeaponInventoryManager();
                    if (weaponInventoryManager != null)
                    {
                        // 모든 아이템의 장착 상태를 동기화
                        for (int i = 0; i < inventoryItems.Count; i++)
                        {
                            WeaponItem item = inventoryItems[i];
                            weaponInventoryManager.UpdateEquipStatus(item.id, i, item.Equip == 1);
                        }
                        
                        // 변경 사항 저장
                        gameDataManager.SaveAllData();
                        Debug.Log("GameDataManager를 통해 장착 상태 변경 완료");
                    }
                    else
                    {
                        Debug.LogError("GameDataManager에서 WeaponInventoryManager를 찾을 수 없습니다.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"GameDataManager를 통한 저장 중 오류: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("GameDataManager가 없어 장착 상태를 전역적으로 저장할 수 없습니다.");
            }
            
            // 저장
            SaveInventoryData();
            
            // 저장 후 파일 내용 확인
            string inventoryPath = Path.Combine(Application.persistentDataPath, "WeaponInventory.json");
            if (File.Exists(inventoryPath))
            {
                string savedContent = File.ReadAllText(inventoryPath);
                Debug.Log($"저장 후 파일 내용: {savedContent}");
                
                // 저장된 내용에서 현재 아이템의 장착 상태 확인
                string targetItemJson = $"{{\"id\":{targetItem.id},\"Equip\":{targetItem.Equip}}}";
                if (savedContent.Contains(targetItemJson))
                {
                    Debug.Log($"아이템 ID {targetItem.id}의 장착 상태 {targetItem.Equip}가 파일에 저장되었습니다.");
                }
                else
                {
                    Debug.LogError($"아이템 ID {targetItem.id}의 장착 상태 {targetItem.Equip}가 파일에 저장되지 않았습니다!");
                    Debug.LogError($"찾을 내용: {targetItemJson}");
                    Debug.LogError($"파일 내용: {savedContent}");
                    
                    // 강제 저장 시도
                    Debug.Log("강제 저장 시도...");
                    try
                    {
                        // 직접 파일 쓰기
                        File.WriteAllText(inventoryPath, savedContent);
                        Debug.Log("강제 저장 완료");
                        
                        // 다시 확인
                        string recheck = File.ReadAllText(inventoryPath);
                        Debug.Log($"강제 저장 후 파일 내용: {recheck}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"강제 저장 중 오류: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.LogError($"저장 후 파일이 존재하지 않습니다: {inventoryPath}");
                
                // 파일이 없으면 다시 저장 시도
                Debug.Log("파일이 없어 다시 저장 시도...");
                try
                {
                    // 간단한 방식으로 JSON 배열 구성
                    List<string> jsonItems = new List<string>();
                    foreach (WeaponItem item in inventoryItems)
                    {
                        jsonItems.Add($"{{\"id\":{item.id},\"Equip\":{item.Equip}}}");
                    }
                    
                    string jsonArray = "[" + string.Join(",", jsonItems) + "]";
                    
                    // 직접 파일 쓰기
                    File.WriteAllText(inventoryPath, jsonArray);
                    Debug.Log("다시 저장 완료");
                    
                    // 확인
                    if (File.Exists(inventoryPath))
                    {
                        string content = File.ReadAllText(inventoryPath);
                        Debug.Log($"다시 저장 후 파일 내용: {content}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"다시 저장 중 오류: {ex.Message}");
                }
            }
            
            // UI 갱신
            GenerateInventoryUI();
        }
        
        return changed;
    }

    // 인벤토리 새로고침 메소드 (외부에서 호출 가능)
    public void RefreshInventory()
    {
        // UI만 다시 생성 (데이터는 다시 로드하지 않음)
        GenerateInventoryUI();
        
        Debug.Log("인벤토리 UI가 새로고침되었습니다.");
    }

    // 파일 권한 확인 메소드
    private void CheckFilePermissions()
    {
        string testPath = Path.Combine(Application.persistentDataPath, "test_write.txt");
        
        try
        {
            // 테스트 파일 쓰기
            File.WriteAllText(testPath, "Test write permission");
            Debug.Log($"파일 쓰기 권한 확인 성공: {testPath}");
            
            // 테스트 파일 읽기
            string content = File.ReadAllText(testPath);
            Debug.Log($"파일 읽기 권한 확인 성공: {content}");
            
            // 테스트 파일 삭제
            File.Delete(testPath);
            Debug.Log("테스트 파일 삭제 성공");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"파일 권한 확인 중 오류: {ex.Message}");
        }
    }

    // GameDataManager에서 WeaponInventoryManager 가져오기
    private BackendData.GameData.WeaponInventory.Manager GetWeaponInventoryManager()
    {
        if (gameDataManager != null)
        {
            try
            {
                // GameDataManager에서 WeaponInventory 매니저 가져오기
                var weaponInventoryManager = gameDataManager.GetWeaponInventoryManager();
                return weaponInventoryManager;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"WeaponInventoryManager 가져오기 오류: {ex.Message}");
            }
        }
        
        return null;
    }
} 