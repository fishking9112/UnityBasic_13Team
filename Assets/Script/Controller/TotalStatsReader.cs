using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// TotalStats.json 파일에서 최종 합산된 스탯 값을 읽어오는 클래스
/// </summary>
public class TotalStatsReader : MonoBehaviour
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

    // 싱글톤 인스턴스
    private static TotalStatsReader _instance;
    public static TotalStatsReader Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("TotalStatsReader");
                _instance = go.AddComponent<TotalStatsReader>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 스탯 데이터
    private TotalStatsData statsData;
    private string statsFilePath;
    private bool isDataLoaded = false;

    // 스탯 프로퍼티
    public int MaxHealth => statsData?.최대체력 ?? 100;
    public int Attack => statsData?.공격력 ?? 10;
    public int Defense => statsData?.방어력 ?? 5;
    public float DamageReduction => statsData?.방어율 ?? 0.1f;
    public float CriticalRate => statsData?.크리티컬율 ?? 0.05f;
    public float MoveSpeed => statsData?.이동속도 ?? 3f;

    private void Awake()
    {
        // 싱글톤 설정
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 파일 경로 설정
        statsFilePath = Path.Combine(Application.persistentDataPath, "TotalStats.json");

        // 초기 데이터 로드
        LoadStatsData();
    }

    /// <summary>
    /// TotalStats.json 파일에서 스탯 데이터를 로드합니다.
    /// </summary>
    /// <returns>로드 성공 여부</returns>
    public bool LoadStatsData()
    {
        try
        {
            if (File.Exists(statsFilePath))
            {
                string json = File.ReadAllText(statsFilePath);
                statsData = JsonUtility.FromJson<TotalStatsData>(json);
                isDataLoaded = true;
                Debug.Log($"TotalStats 데이터 로드 완료: 체력 {statsData.최대체력}, 공격력 {statsData.공격력}, 방어력 {statsData.방어력}");
                return true;
            }
            else
            {
                // 파일이 없으면 기본값으로 초기화하지만 StatHandler에서 곧 생성할 것임
                statsData = new TotalStatsData
                {
                    최대체력 = 100,
                    공격력 = 10,
                    방어력 = 5,
                    방어율 = 0.1f,
                    크리티컬율 = 0.05f,
                    이동속도 = 3f
                };
                Debug.LogWarning("TotalStats 파일이 없어 기본값으로 설정됨 - StatHandler가 곧 정확한 값을 계산할 것임");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TotalStats 데이터 로드 중 오류: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 스탯 데이터가 로드되었는지 확인합니다.
    /// </summary>
    public bool IsDataLoaded()
    {
        return isDataLoaded;
    }

    /// <summary>
    /// 모든 스탯 데이터를 한 번에 가져옵니다.
    /// </summary>
    public TotalStatsData GetAllStats()
    {
        if (!isDataLoaded)
        {
            LoadStatsData();
        }
        return statsData;
    }

    /// <summary>
    /// 스탯 데이터를 다시 로드합니다. StatHandler에서 스탯이 업데이트된 후 호출하세요.
    /// </summary>
    public void RefreshStats()
    {
        LoadStatsData();
        Debug.Log("TotalStats 데이터 새로고침 완료");
    }

    /// <summary>
    /// 특정 스탯 값을 이름으로 가져옵니다.
    /// </summary>
    /// <param name="statName">스탯 이름 (최대체력, 공격력, 방어력, 방어율, 크리티컬율, 이동속도)</param>
    /// <returns>스탯 값 (float)</returns>
    public float GetStatByName(string statName)
    {
        if (!isDataLoaded)
        {
            LoadStatsData();
        }

        switch (statName)
        {
            case "최대체력":
                return statsData.최대체력;
            case "공격력":
                return statsData.공격력;
            case "방어력":
                return statsData.방어력;
            case "방어율":
                return statsData.방어율;
            case "크리티컬율":
                return statsData.크리티컬율;
            case "이동속도":
                return statsData.이동속도;
            default:
                Debug.LogWarning($"알 수 없는 스탯 이름: {statName}");
                return 0;
        }
    }

    /// <summary>
    /// 현재 스탯 정보를 문자열로 반환합니다.
    /// </summary>
    public string GetStatsInfoString()
    {
        if (!isDataLoaded)
        {
            LoadStatsData();
        }

        return $"최대체력: {statsData.최대체력}\n" +
               $"공격력: {statsData.공격력}\n" +
               $"방어력: {statsData.방어력}\n" +
               $"방어율: {statsData.방어율 * 100:F1}%\n" +
               $"크리티컬율: {statsData.크리티컬율 * 100:F1}%\n" +
               $"이동속도: {statsData.이동속도:F1}";
    }
} 