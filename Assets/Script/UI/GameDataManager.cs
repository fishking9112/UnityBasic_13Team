using UnityEngine;
using BackendData.GameData;
using System;
using System.Reflection;
using System.IO;

public class GameDataManager : MonoBehaviour
{
    private static GameDataManager instance;
    
    public static GameDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameDataManager");
                instance = go.AddComponent<GameDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    // 게임 데이터
    private UserData userData;
    private BackendData.GameData.WeaponInventory.Manager weaponInventory;
    public BackendData.GameData.QuestAchievement.Manager QuestAchievement { get; private set; }

    // 프로퍼티
    public UserData UserData => userData;
    public BackendData.GameData.WeaponInventory.Manager WeaponInventory => weaponInventory;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 데이터 초기화
        InitializeData();
    }
    
    private void InitializeData()
    {
        // 유저 데이터 초기화
        userData = new UserData();
        bool userDataExists = userData.LoadFromJson();
        
        if (!userDataExists)
        {
            Debug.Log("유저 데이터가 없습니다. 새로 생성합니다.");
            userData.Initialize(); // 공개 메서드 호출
            userData.SaveToJson(); // 즉시 저장
        }
        
        // 무기 인벤토리 초기화
        weaponInventory = new BackendData.GameData.WeaponInventory.Manager();
        weaponInventory.LoadFromJson();
        
        // QuestAchievement 초기화
        QuestAchievement = new BackendData.GameData.QuestAchievement.Manager();
        QuestAchievement.CreateNewData();
    }

    void OnEnable()
    {
        // 게임 종료시 데이터 저장을 위해 이벤트 등록
        Application.quitting += SaveAllData;
    }

    void OnDisable()
    {
        Application.quitting -= SaveAllData;
    }

    public void CreateNewUserData()
    {
        try
        {
            userData = new UserData();
            weaponInventory = new BackendData.GameData.WeaponInventory.Manager();
            QuestAchievement = new BackendData.GameData.QuestAchievement.Manager();

            // 데이터 생성
            userData.CreateNewData();
            Debug.Log("UserData 생성 완료");
            
            // 유저 데이터 명시적 저장
            userData.SaveToJson();
            Debug.Log("UserData 저장 완료");
            
            weaponInventory.CreateNewData();
            QuestAchievement.CreateNewData();
            
            // 초기 골드 설정 (예: 1000)
            AddGold(1000);
            
            // 각 데이터 개별 저장 - 저장 경로 출력
            Debug.Log("=== 데이터 저장 경로 확인 ===");
            
            // UserData 저장
            FieldInfo pathField = userData.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField != null)
            {
                string userDataPath = (string)pathField.GetValue(userData);
                Debug.Log($"UserData 저장 경로: {userDataPath}");
            }
            userData.SaveToJson();
            Debug.Log("UserData 저장 완료");
            
            // WeaponInventory 저장
            pathField = weaponInventory.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField != null)
            {
                string weaponPath = (string)pathField.GetValue(weaponInventory);
                Debug.Log($"WeaponInventory 저장 경로: {weaponPath}");
            }
            weaponInventory.SaveToJson();
            Debug.Log("WeaponInventory 저장 완료");
            
            // QuestAchievement 저장
            pathField = QuestAchievement.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField != null)
            {
                string questPath = (string)pathField.GetValue(QuestAchievement);
                Debug.Log($"QuestAchievement 저장 경로: {questPath}");
            }
            QuestAchievement.SaveToJson();
            Debug.Log("QuestAchievement 저장 완료");
            
            // 변경 사항 저장 (통합 저장)
            SaveAllData();
            
            // 저장 후 파일 존재 확인
            CheckDataFiles();
            
            Debug.Log("새 유저 데이터가 생성되고 저장되었습니다.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"유저 데이터 생성 중 오류: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    // 데이터 파일 존재 확인 메서드 추가
    private void CheckDataFiles()
    {
        try
        {
            // UserData 파일 확인
            FieldInfo pathField = userData.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField != null)
            {
                string userDataPath = (string)pathField.GetValue(userData);
                bool userDataExists = File.Exists(userDataPath);
                Debug.Log($"UserData 파일 존재: {userDataExists}, 경로: {userDataPath}");
                
                if (userDataExists)
                {
                    string content = File.ReadAllText(userDataPath);
                    Debug.Log($"UserData 파일 내용: {content.Substring(0, Math.Min(100, content.Length))}...");
                }
            }
            
            // WeaponInventory 파일 확인
            pathField = weaponInventory.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField != null)
            {
                string weaponPath = (string)pathField.GetValue(weaponInventory);
                bool weaponDataExists = File.Exists(weaponPath);
                Debug.Log($"WeaponInventory 파일 존재: {weaponDataExists}, 경로: {weaponPath}");
                
                if (weaponDataExists)
                {
                    string content = File.ReadAllText(weaponPath);
                    Debug.Log($"WeaponInventory 파일 내용: {content.Substring(0, Math.Min(100, content.Length))}...");
                }
            }
            
            // QuestAchievement 파일 확인
            pathField = QuestAchievement.GetType().GetField("_filePath", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pathField != null)
            {
                string questPath = (string)pathField.GetValue(QuestAchievement);
                bool questDataExists = File.Exists(questPath);
                Debug.Log($"QuestAchievement 파일 존재: {questDataExists}, 경로: {questPath}");
                
                if (questDataExists)
                {
                    string content = File.ReadAllText(questPath);
                    Debug.Log($"QuestAchievement 파일 내용: {content.Substring(0, Math.Min(100, content.Length))}...");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 파일 확인 중 오류: {ex.Message}");
        }
    }

    public void LoadExistingUserData()
    {
        try
        {
            userData = new UserData();
            weaponInventory = new BackendData.GameData.WeaponInventory.Manager();
            QuestAchievement = new BackendData.GameData.QuestAchievement.Manager();

            // 로그 출력 줄이기 - 로드 시 로그 출력 제거
            bool originalLogState = BackendData.Base.GameData.EnableDetailedLogs;
            BackendData.Base.GameData.EnableDetailedLogs = false;
            
            userData.LoadFromJson();
            weaponInventory.LoadFromJson();
            QuestAchievement.LoadFromJson();
            
            // 로그 상태 복원
            BackendData.Base.GameData.EnableDetailedLogs = originalLogState;
            
            Debug.Log("모든 게임 데이터를 로드했습니다.");
            
            // 저장 시에도 로그 출력 줄이기
            originalLogState = BackendData.Base.GameData.EnableDetailedLogs;
            BackendData.Base.GameData.EnableDetailedLogs = false;
            
            // 여기서 SaveToJson을 직접 호출하여 확실하게 저장
            userData.SaveToJson();
            weaponInventory.SaveToJson();
            QuestAchievement.SaveToJson();
            
            // 그 후 SaveAllData 호출
            SaveAllData();
            
            // 로그 상태 복원
            BackendData.Base.GameData.EnableDetailedLogs = originalLogState;
            Debug.Log("모든 게임 데이터를 저장했습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 로드 중 오류 발생: {e.Message}");
            throw;
        }
    }

    public void SaveAllData()
    {
        try
        {
            // 로그 출력 줄이기
            bool originalLogState = BackendData.Base.GameData.EnableDetailedLogs;
            BackendData.Base.GameData.EnableDetailedLogs = false;
            
            // 각 데이터 저장
            if (userData != null)
            {
                userData.SaveToJson();
                Debug.Log("유저 데이터 저장 완료");
            }
            
            if (weaponInventory != null)
            {
                weaponInventory.SaveToJson();
                Debug.Log("무기 인벤토리 저장 완료");
            }
            
            if (QuestAchievement != null)
            {
                QuestAchievement.SaveToJson();
                Debug.Log("퀘스트 데이터 저장 완료");
            }
            
            // 로그 상태 복원
            BackendData.Base.GameData.EnableDetailedLogs = originalLogState;
            
            Debug.Log("모든 게임 데이터를 저장했습니다.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 저장 중 오류: {ex.Message}");
        }
    }

    // 무기 장착 상태 변경 메서드 추가
    public void EquipWeapon(int weaponId, bool isEquipped)
    {
        // 이전에 장착된 무기가 있으면 해제
        if (isEquipped)
        {
            // 같은 타입의 무기를 찾아서 해제
            var allWeapons = weaponInventory.GetAllWeaponItems();
            foreach (var weapon in allWeapons)
            {
                if (weapon.id != weaponId && weapon.Equip == 1)
                {
                    // 같은 타입인지 확인 (첫 자리 숫자로 타입 구분)
                    int currentType = weapon.id / 1000;
                    int newType = weaponId / 1000;
                    
                    if (currentType == newType)
                    {
                        weaponInventory.SetEquipStatus(weapon.id, 0);
                    }
                }
            }
        }
        
        // 새 무기 장착 상태 변경
        weaponInventory.SetEquipStatus(weaponId, isEquipped ? 1 : 0);
        
        // 변경사항 저장
        weaponInventory.SaveToJson();
    }
    
    // 장착된 무기 ID 가져오기 (타입별)
    public int GetEquippedWeaponId(int type)
    {
        var allWeapons = weaponInventory.GetAllWeaponItems();
        foreach (var weapon in allWeapons)
        {
            // 타입 확인 (첫 자리 숫자로 타입 구분)
            int weaponType = weapon.id / 1000;
            
            if (weaponType == type && weapon.Equip == 1)
            {
                return weapon.id;
            }
        }
        
        return 0; // 장착된 무기가 없음
    }

    // 골드 추가/차감 메서드 수정
    public void AddGold(int amount)
    {
        if (userData != null)
        {
            try
            {
                userData.AddGold(amount);
                userData.SaveToJson();
            }
            catch (Exception ex)
            {
                Debug.LogError($"골드 추가/차감 중 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    // 골드 차감 메서드 (편의를 위해)
    public void ReduceGold(int amount)
    {
        AddGold(-amount);
    }

    // WeaponInventoryManager 가져오기
    public BackendData.GameData.WeaponInventory.Manager GetWeaponInventoryManager()
    {
        return weaponInventory;
    }

    // 특정 아이템의 장착 상태 업데이트
    public void UpdateWeaponEquipStatus(int itemId, int itemIndex, bool isEquipped)
    {
        if (weaponInventory != null)
        {
            weaponInventory.UpdateEquipStatus(itemId, itemIndex, isEquipped);
            SaveAllData();
        }
    }
} 