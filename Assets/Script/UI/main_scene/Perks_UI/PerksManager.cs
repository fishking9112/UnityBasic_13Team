using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PerksManager : MonoBehaviour
{
    [System.Serializable]
    public class StatsData
    {
        public int 공격력;
        public int 방어력;
        public int 경감;
        public int 크리티컬;
    }

    [Header("UI References")]
    [SerializeField] private Button[] statButtons; // 4개의 스탯 버튼
    [SerializeField] private Text[] statLevelTexts; // 각 버튼에 표시될 레벨 텍스트
    [SerializeField] private Button levelUpButton; // 레벨업 버튼
    [SerializeField] private Text titleText; // "재능" 타이틀
    [SerializeField] private Text pointsText; // "0회 업그레이드"

    [Header("Settings")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f); // 선택된 버튼 색상
    [SerializeField] private Color normalColor = Color.white; // 기본 버튼 색상

    private StatsData statsData;
    private string statsFilePath;
    private int selectedStatIndex = -1; // 현재 선택된 스탯 인덱스 (-1은 선택 안됨)
    private int upgradePoints = 0; // 사용 가능한 업그레이드 포인트

    // 스탯 이름 배열 (인덱스 순서대로)
    private readonly string[] statNames = { "공격력", "방어력", "경감", "크리티컬" };

    private void Start()
    {
        // 스탯 데이터 파일 경로 설정
        statsFilePath = Path.Combine(Application.persistentDataPath, "Stats_data.json");
        
        // 스탯 데이터 로드
        LoadStatsData();
        
        // UI 초기화
        InitializeUI();
        
        // 버튼 이벤트 등록
        RegisterButtonEvents();
        
        // 사용 가능한 업그레이드 포인트 로드 (실제 구현에 맞게 수정 필요)
        LoadUpgradePoints();
        
        // 포인트 텍스트 업데이트
        UpdatePointsText();
    }

    private void LoadStatsData()
    {
        try
        {
            // 파일이 존재하는지 확인
            if (File.Exists(statsFilePath))
            {
                // 파일에서 JSON 읽기
                string json = File.ReadAllText(statsFilePath);
                statsData = JsonUtility.FromJson<StatsData>(json);
                Debug.Log("스탯 데이터 로드 완료");
            }
            else
            {
                // 파일이 없으면 기본값으로 초기화
                statsData = new StatsData();
                SaveStatsData();
                Debug.Log("스탯 데이터 파일이 없어 기본값으로 생성");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"스탯 데이터 로드 중 오류: {e.Message}");
            statsData = new StatsData();
        }
    }

    private void SaveStatsData()
    {
        try
        {
            // 객체를 JSON으로 변환
            string json = JsonUtility.ToJson(statsData, true);
            
            // JSON을 파일에 저장
            File.WriteAllText(statsFilePath, json);
            Debug.Log("스탯 데이터 저장 완료");
            
            // 차트 폴더에도 저장 (개발용)
            string chartPath = Path.Combine(Application.dataPath, "Script/Chart/Stats_data.json");
            File.WriteAllText(chartPath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"스탯 데이터 저장 중 오류: {e.Message}");
        }
    }

    private void InitializeUI()
    {
        // 각 스탯 버튼에 현재 레벨 표시
        UpdateStatLevelTexts();
        
        // 레벨업 버튼 초기 상태 설정
        UpdateLevelUpButtonState();
    }

    private void RegisterButtonEvents()
    {
        // 스탯 버튼 이벤트 등록
        for (int i = 0; i < statButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            statButtons[i].onClick.AddListener(() => OnStatButtonClicked(index));
        }
        
        // 레벨업 버튼 이벤트 등록
        levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
    }

    private void OnStatButtonClicked(int index)
    {
        // 이미 선택된 버튼을 다시 클릭하면 선택 해제
        if (selectedStatIndex == index)
        {
            selectedStatIndex = -1;
        }
        else
        {
            selectedStatIndex = index;
        }
        
        // 버튼 색상 업데이트
        UpdateButtonColors();
        
        // 레벨업 버튼 상태 업데이트
        UpdateLevelUpButtonState();
        
        Debug.Log($"스탯 선택: {statNames[index]}, 현재 레벨: {GetStatLevel(index)}");
    }

    private void OnLevelUpButtonClicked()
    {
        if (selectedStatIndex >= 0 && selectedStatIndex < statNames.Length && upgradePoints > 0)
        {
            // 선택된 스탯 레벨 증가
            IncreaseStatLevel(selectedStatIndex);
            
            // 업그레이드 포인트 감소
            upgradePoints--;
            
            // UI 업데이트
            UpdateStatLevelTexts();
            UpdatePointsText();
            UpdateLevelUpButtonState();
            
            // 데이터 저장
            SaveStatsData();
            SaveUpgradePoints();
            
            Debug.Log($"{statNames[selectedStatIndex]} 레벨 증가: {GetStatLevel(selectedStatIndex)}");
        }
    }

    private int GetStatLevel(int index)
    {
        switch (index)
        {
            case 0: return statsData.공격력;
            case 1: return statsData.방어력;
            case 2: return statsData.경감;
            case 3: return statsData.크리티컬;
            default: return 0;
        }
    }

    private void IncreaseStatLevel(int index)
    {
        switch (index)
        {
            case 0: statsData.공격력++; break;
            case 1: statsData.방어력++; break;
            case 2: statsData.경감++; break;
            case 3: statsData.크리티컬++; break;
        }
    }

    private void UpdateStatLevelTexts()
    {
        for (int i = 0; i < statLevelTexts.Length; i++)
        {
            if (i < statNames.Length)
            {
                statLevelTexts[i].text = GetStatLevel(i).ToString();
            }
        }
    }

    private void UpdateButtonColors()
    {
        for (int i = 0; i < statButtons.Length; i++)
        {
            Image buttonImage = statButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = (i == selectedStatIndex) ? selectedColor : normalColor;
            }
        }
    }

    private void UpdateLevelUpButtonState()
    {
        // 스탯이 선택되어 있고 업그레이드 포인트가 있을 때만 활성화
        levelUpButton.interactable = (selectedStatIndex >= 0 && upgradePoints > 0);
    }

    private void UpdatePointsText()
    {
        pointsText.text = $"{upgradePoints}회 업그레이드";
    }

    private void LoadUpgradePoints()
    {
        // 실제 구현에 맞게 수정 필요
        // 예: PlayerPrefs나 다른 저장 시스템에서 포인트 로드
        string pointsPath = Path.Combine(Application.persistentDataPath, "UpgradePoints.json");
        
        try
        {
            if (File.Exists(pointsPath))
            {
                string json = File.ReadAllText(pointsPath);
                upgradePoints = JsonUtility.FromJson<UpgradePointsData>(json).points;
            }
            else
            {
                // 테스트용 기본값
                upgradePoints = 5;
                SaveUpgradePoints();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업그레이드 포인트 로드 중 오류: {e.Message}");
            upgradePoints = 5; // 오류 시 기본값
        }
    }

    private void SaveUpgradePoints()
    {
        // 실제 구현에 맞게 수정 필요
        string pointsPath = Path.Combine(Application.persistentDataPath, "UpgradePoints.json");
        
        try
        {
            UpgradePointsData data = new UpgradePointsData { points = upgradePoints };
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(pointsPath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"업그레이드 포인트 저장 중 오류: {e.Message}");
        }
    }

    [System.Serializable]
    private class UpgradePointsData
    {
        public int points;
    }

    // 테스트용 메소드: 업그레이드 포인트 추가
    public void AddUpgradePoint()
    {
        upgradePoints++;
        UpdatePointsText();
        UpdateLevelUpButtonState();
        SaveUpgradePoints();
    }
} 