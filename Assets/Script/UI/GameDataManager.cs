using UnityEngine;
using BackendData.GameData;
using BackendData.GameData.WeaponInventory;
using BackendData.GameData.WeaponEquip;
using BackendData.GameData.QuestAchievement;
using System;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public UserData UserData { get; private set; }
    public BackendData.GameData.WeaponInventory.Manager WeaponInventory { get; private set; }
    public BackendData.GameData.WeaponEquip.Manager WeaponEquip { get; private set; }
    public BackendData.GameData.QuestAchievement.Manager QuestAchievement { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
            UserData = new UserData();
            WeaponInventory = new BackendData.GameData.WeaponInventory.Manager();
            WeaponEquip = new BackendData.GameData.WeaponEquip.Manager();
            QuestAchievement = new BackendData.GameData.QuestAchievement.Manager();

            UserData.CreateNewData();
            WeaponInventory.CreateNewData();
            WeaponEquip.CreateNewData();
            QuestAchievement.CreateNewData();
            
            SaveAllData();
        }
        catch
        {
            throw;
        }
    }

    public void LoadExistingUserData()
    {
        try
        {
            UserData = new UserData();
            WeaponInventory = new BackendData.GameData.WeaponInventory.Manager();
            WeaponEquip = new BackendData.GameData.WeaponEquip.Manager();
            QuestAchievement = new BackendData.GameData.QuestAchievement.Manager();

            UserData.LoadFromJson();
            WeaponInventory.LoadFromJson();
            WeaponEquip.LoadFromJson();
            QuestAchievement.LoadFromJson();

            SaveAllData();
            Debug.Log("모든 게임 데이터를 로드했습니다.");
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
            if (UserData != null)
            {
                UserData.ForceSetChanged();
                UserData.SaveToJson();
            }
            
            if (WeaponInventory != null)
            {
                WeaponInventory.ForceSetChanged();
                WeaponInventory.SaveToJson();
            }
            
            if (WeaponEquip != null)
            {
                WeaponEquip.ForceSetChanged();
                WeaponEquip.SaveToJson();
            }
            
            if (QuestAchievement != null)
            {
                QuestAchievement.ForceSetChanged();
                QuestAchievement.SaveToJson();
            }
        }
        catch
        {
            throw;
        }
    }
} 