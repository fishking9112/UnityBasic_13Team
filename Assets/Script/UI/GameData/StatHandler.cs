using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StatHandler : MonoBehaviour
{
    [System.Serializable]
    public class TotalStatsData
    {
        public int 최대체력;
        public int 공격력;
        public int 방어력;
        public float 방어율;
        public float 크리티컬율;
        public float 이동속도;
    }
    
    [System.Serializable]
    public class WeaponData
    {
        public List<WeaponItem> items;
    }
    
    [System.Serializable]
    public class WeaponItem
    {
        public int id;
        public string name;
        public int capability_value;
        public int Type;
        public int rating;
    }
    
    [System.Serializable]
    public class WeaponInventoryItem
    {
        public int id;
        public int Equip;
    }

    [Header("Base Stats")]
    [Range(1, 1000)][SerializeField] private int baseHealth = 100;
    [Range(1, 100)][SerializeField] private int baseAttack = 10;
    [Range(1, 100)][SerializeField] private int baseDefense = 5;
    [Range(0f, 1f)][SerializeField] private float baseDamageReduction = 0.1f;
    [Range(0f, 1f)][SerializeField] private float baseCriticalRate = 0.05f;
    [Range(1f, 20f)][SerializeField] private float baseSpeed = 3f;

    [Header("Stat Multipliers")]
    [SerializeField] private float perkStatMultiplier = 75f; // 특전 스탯 1레벨당 증가량

    // 최종 계산된 스탯
    private int finalHealth;
    private int finalAttack;
    private int finalDefense;
    private float finalDamageReduction;
    private float finalCriticalRate;
    private float finalSpeed;

    // 스탯 데이터 파일 경로
    private string totalStatsFilePath;
    private string weaponDataPath;
    private string weaponInventoryPath;
    private string perkStatsPath;
    private string userDataPath;
    
    // 무기 데이터 캐시
    private Dictionary<int, WeaponItem> weaponDataDict = new Dictionary<int, WeaponItem>();

    private UserData userData;

    [System.Serializable]
    public class UserData
    {
        public List<string> keys;
        public List<string> values;
        
        public string GetValue(string key)
        {
            int index = keys.IndexOf(key);
            if (index >= 0 && index < values.Count)
                return values[index];
            return null;
        }
    }

    // 프로퍼티
    public int Health => finalHealth;
    public int Attack => finalAttack;
    public int Defense => finalDefense;
    public float DamageReduction => finalDamageReduction;
    public float CriticalRate => finalCriticalRate;
    public float Speed => finalSpeed;

    private void Awake()
    {
        // 파일 경로 설정
        totalStatsFilePath = Path.Combine(Application.persistentDataPath, "TotalStats.json");
        weaponDataPath = Path.Combine(Application.dataPath, "Script/Chart/Weapon_data.json");
        weaponInventoryPath = Path.Combine(Application.persistentDataPath, "WeaponInventory.json");
        perkStatsPath = Path.Combine(Application.persistentDataPath, "Stats_data.json");
        userDataPath = Path.Combine(Application.persistentDataPath, "UserData.json");
        
        // 무기 데이터 로드
        LoadWeaponData();
        
        // 유저 데이터 로드
        LoadUserData();
        
        // 스탯 계산 및 초기화
        CalculateStats();
    }
    
    private void LoadWeaponData()
    {
        try
        {
            if (File.Exists(weaponDataPath))
            {
                string json = File.ReadAllText(weaponDataPath);
                WeaponData weaponData = JsonUtility.FromJson<WeaponData>(json);
                
                // 딕셔너리에 무기 데이터 저장
                weaponDataDict.Clear();
                foreach (var item in weaponData.items)
                {
                    weaponDataDict[item.id] = item;
                }
                
                Debug.Log($"무기 데이터 로드 완료: {weaponDataDict.Count}개 아이템");
            }
            else
            {
                Debug.LogError("무기 데이터 파일을 찾을 수 없습니다: " + weaponDataPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"무기 데이터 로드 중 오류: {e.Message}");
        }
    }

    private void LoadUserData()
    {
        try
        {
            if (File.Exists(userDataPath))
            {
                string json = File.ReadAllText(userDataPath);
                userData = JsonUtility.FromJson<UserData>(json);
                Debug.Log("유저 데이터 로드 완료");
            }
            else
            {
                Debug.LogWarning("유저 데이터 파일을 찾을 수 없습니다: " + userDataPath);
                userData = new UserData(); // 기본값 생성
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"유저 데이터 로드 중 오류: {e.Message}");
            userData = new UserData(); // 오류 시 기본값 생성
        }
    }

    public void CalculateStats()
    {
        // 1. 유저 데이터에서 기본 스탯 설정 (없으면 기본값 사용)
        finalHealth = baseHealth;
        finalAttack = baseAttack;
        finalDefense = baseDefense;
        finalDamageReduction = baseDamageReduction;
        finalCriticalRate = baseCriticalRate;
        finalSpeed = baseSpeed;
        
        // UserData.json에서 값 가져오기
        if (userData != null)
        {
            string attackStr = userData.GetValue("Attack");
            string defenseStr = userData.GetValue("Defense");
            string defenseRateStr = userData.GetValue("DefenseRate");
            string criticalRateStr = userData.GetValue("CriticalRate");
            
            if (!string.IsNullOrEmpty(attackStr)) 
                finalAttack = int.Parse(attackStr);
            
            if (!string.IsNullOrEmpty(defenseStr)) 
                finalDefense = int.Parse(defenseStr);
            
            if (!string.IsNullOrEmpty(defenseRateStr)) 
                finalDamageReduction = float.Parse(defenseRateStr);
            
            if (!string.IsNullOrEmpty(criticalRateStr)) 
                finalCriticalRate = float.Parse(criticalRateStr);
            
            Debug.Log($"유저 데이터에서 로드한 스탯: 공격력 {finalAttack}, 방어력 {finalDefense}, " +
                     $"방어율 {finalDamageReduction:F2}, 크리티컬율 {finalCriticalRate:F2}");
        }

        // 2. 특전 스탯(Stats_data.json) 적용
        ApplyPerkStats();

        // 3. 장착 아이템(WeaponInventory.json) 스탯 적용
        ApplyEquippedItemStats();

        // 4. 최종 스탯 저장
        SaveTotalStats();
        
        // 5. 디버그 로그
        Debug.Log($"최종 스탯 계산 완료: 체력 {finalHealth}, 공격력 {finalAttack}, 방어력 {finalDefense}, " +
                 $"방어율 {finalDamageReduction:F2}, 크리티컬율 {finalCriticalRate:F2}, 이동속도 {finalSpeed:F1}");
    }

    private void ApplyPerkStats()
    {
        try
        {
            if (File.Exists(perkStatsPath))
            {
                string json = File.ReadAllText(perkStatsPath);
                var perkStats = JsonUtility.FromJson<PerksManager.StatsData>(json);

                // 특전 스탯 적용 (각 레벨당 perkStatMultiplier만큼 증가)
                finalAttack += Mathf.RoundToInt(perkStats.공격력 * perkStatMultiplier);
                finalDefense += Mathf.RoundToInt(perkStats.방어력 * perkStatMultiplier);
                finalDamageReduction += perkStats.경감 * 0.01f; // 1레벨당 1% 증가
                finalCriticalRate += perkStats.크리티컬 * 0.01f; // 1레벨당 1% 증가

                Debug.Log($"특전 스탯 적용: 공격력 +{perkStats.공격력 * perkStatMultiplier}, 방어력 +{perkStats.방어력 * perkStatMultiplier}, " +
                         $"방어율 +{perkStats.경감 * 0.01f:F2}, 크리티컬율 +{perkStats.크리티컬 * 0.01f:F2}");
            }
            else
            {
                Debug.LogWarning("특전 스탯 파일을 찾을 수 없습니다: " + perkStatsPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"특전 스탯 적용 중 오류: {e.Message}");
        }
    }

    private void ApplyEquippedItemStats()
    {
        try
        {
            if (File.Exists(weaponInventoryPath))
            {
                string json = File.ReadAllText(weaponInventoryPath);
                List<WeaponInventoryItem> inventoryItems = new List<WeaponInventoryItem>();
                
                // JSON 배열 파싱
                if (json.StartsWith("[") && json.EndsWith("]"))
                {
                    // 배열 형식의 JSON을 List<WeaponInventoryItem>으로 변환
                    json = "{\"items\":" + json + "}";
                    var wrapper = JsonUtility.FromJson<InventoryWrapper>(json);
                    inventoryItems = wrapper.items;
                    
                    // 디버그: 인벤토리 아이템 수 확인
                    Debug.Log($"인벤토리에서 로드된 아이템 수: {inventoryItems.Count}");
                }
                
                // 장착된 아이템 찾기
                int equippedCount = 0;
                foreach (var item in inventoryItems)
                {
                    if (item.Equip == 1 && weaponDataDict.ContainsKey(item.id))
                    {
                        // 장착된 아이템의 스탯 적용
                        WeaponItem weaponItem = weaponDataDict[item.id];
                        ApplyItemStat(weaponItem);
                        equippedCount++;
                        
                        // 디버그: 장착된 아이템 정보
                        Debug.Log($"장착된 아이템 발견: ID {item.id}, 이름 {weaponItem.name}, 타입 {weaponItem.Type}, 등급 {weaponItem.rating}");
                    }
                }
                
                // 디버그: 장착된 아이템 총 개수
                Debug.Log($"장착된 아이템 총 개수: {equippedCount}");
            }
            else
            {
                Debug.LogWarning("인벤토리 파일을 찾을 수 없습니다: " + weaponInventoryPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"장착 아이템 스탯 적용 중 오류: {e.Message}");
        }
    }
    
    [System.Serializable]
    private class InventoryWrapper
    {
        public List<WeaponInventoryItem> items;
    }

    private void ApplyItemStat(WeaponItem item)
    {
        // 아이템 적용 전 스탯 기록 (디버그용)
        int prevAttack = finalAttack;
        int prevDefense = finalDefense;
        int prevHealth = finalHealth;
        float prevDamageReduction = finalDamageReduction;
        float prevCriticalRate = finalCriticalRate;
        float prevSpeed = finalSpeed;
        
        // 아이템 타입에 따른 스탯 증가
        switch (item.Type)
        {
            case 1: // 무기
                finalAttack += item.capability_value;
                // 등급에 따른 추가 효과
                if (item.rating >= 3) finalCriticalRate += 0.05f;
                if (item.rating >= 4) finalCriticalRate += 0.05f;
                break;
                
            case 2: // 방어구
                finalDefense += item.capability_value / 10; // 방어력은 capability_value의 10%
                finalHealth += item.capability_value;
                // 등급에 따른 추가 효과
                if (item.rating >= 3) finalDamageReduction += 0.02f;
                if (item.rating >= 4) finalDamageReduction += 0.03f;
                break;
                
            case 3: // 장신구
                finalCriticalRate += item.capability_value * 0.0005f; // 크리티컬율 증가
                finalSpeed += item.capability_value * 0.001f; // 이동속도 증가
                // 등급에 따른 추가 효과
                if (item.rating >= 3) finalDamageReduction += 0.01f;
                if (item.rating >= 4) finalAttack += item.capability_value / 2;
                break;
        }
        
        // 아이템 적용 후 스탯 변화 상세 로그
        Debug.Log($"아이템 '{item.name}' 스탯 적용 상세: " +
                  $"공격력 {prevAttack} → {finalAttack} (+{finalAttack - prevAttack}), " +
                  $"방어력 {prevDefense} → {finalDefense} (+{finalDefense - prevDefense}), " +
                  $"체력 {prevHealth} → {finalHealth} (+{finalHealth - prevHealth}), " +
                  $"방어율 {prevDamageReduction:F3} → {finalDamageReduction:F3} (+{finalDamageReduction - prevDamageReduction:F3}), " +
                  $"크리티컬율 {prevCriticalRate:F3} → {finalCriticalRate:F3} (+{finalCriticalRate - prevCriticalRate:F3}), " +
                  $"이동속도 {prevSpeed:F3} → {finalSpeed:F3} (+{finalSpeed - prevSpeed:F3})");
    }

    private void SaveTotalStats()
    {
        try
        {
            TotalStatsData totalStats = new TotalStatsData
            {
                최대체력 = finalHealth,
                공격력 = finalAttack,
                방어력 = finalDefense,
                방어율 = finalDamageReduction,
                크리티컬율 = finalCriticalRate,
                이동속도 = finalSpeed
            };
            
            string json = JsonUtility.ToJson(totalStats, true);
            File.WriteAllText(totalStatsFilePath, json);
            
            // 차트 폴더에도 저장 (개발용)
            string chartPath = Path.Combine(Application.dataPath, "Script/Chart/TotalStats.json");
            File.WriteAllText(chartPath, json);
            
            Debug.Log($"최종 스탯 저장 완료: 체력 {finalHealth}, 공격력 {finalAttack}, 방어력 {finalDefense}, " +
                     $"방어율 {finalDamageReduction:F2}, 크리티컬율 {finalCriticalRate:F2}, 이동속도 {finalSpeed:F1}");
            
            // TotalStatsReader가 있다면 새로고침 요청
            if (TotalStatsReader.Instance != null)
            {
                TotalStatsReader.Instance.RefreshStats();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"최종 스탯 저장 중 오류: {e.Message}");
        }
    }

    // 스탯 새로고침 (외부에서 호출 가능)
    public void RefreshStats()
    {
        LoadWeaponData(); // 무기 데이터 다시 로드
        CalculateStats();
    }

    // 데미지 계산 (방어력과 방어율 적용)
    public int CalculateDamage(int incomingDamage)
    {
        // 방어력으로 데미지 감소
        float damageAfterDefense = incomingDamage - finalDefense;
        
        // 방어율로 데미지 감소 (퍼센트)
        float finalDamage = damageAfterDefense * (1 - finalDamageReduction);
        
        // 최소 1의 데미지는 입음
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }

    // 크리티컬 여부 확인
    public bool IsCritical()
    {
        return Random.value <= finalCriticalRate;
    }

    private void Start()
    {
        Debug.Log("StatHandler 시작됨");
        // 스탯 계산 및 초기화 (Awake에서 이미 호출했다면 중복 호출 방지)
        if (finalHealth == 0) // 아직 초기화되지 않았다면
        {
            CalculateStats();
        }
    }
}
