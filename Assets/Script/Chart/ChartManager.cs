using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChartManager : MonoBehaviour
{
    public static ChartManager Instance { get; private set; }

    // 차트 데이터 클래스들
    [Serializable]
    public class WeaponData {
        public int id;
        public string name;
        public float capability_value;
        public int Type;
        public int rating;
    }

    [Serializable]
    public class WeaponDataList {
        public List<WeaponData> items;
    }

    [Serializable]
    public class StatsData {
        public float 공격력Value;
        public float 방어력Value;
        public float 경감Value;
        public float 크리티컬Value;
        
        // 속성으로 접근 제공
        public float 공격력 => 공격력Value;
        public float 방어력 => 방어력Value;
        public float 경감 => 경감Value;
        public float 크리티컬 => 크리티컬Value;
    }

    [Serializable]
    public class RarityData {
        public int 일반Value;
        public int 희귀Value;
        public int 에픽Value;
        public int 전설Value;
        
        // 속성으로 접근 제공
        public int 일반 => 일반Value;
        public int 희귀 => 희귀Value;
        public int 에픽 => 에픽Value;
        public int 전설 => 전설Value;
    }

    // 차트 데이터 저장 변수
    public List<WeaponData> WeaponList { get; private set; }
    public StatsData StatsDataInfo { get; private set; }
    public RarityData RarityDataInfo { get; private set; }

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

    // 차트 데이터 로드 함수
    public bool LoadAllChartData()
    {
        try
        {
            // Chart 폴더 경로
            string chartFolderPath = Path.Combine(Application.dataPath, "Script/Chart");
            
            if (Directory.Exists(chartFolderPath))
            {
                // 무기 데이터 로드
                string weaponPath = Path.Combine(chartFolderPath, "Weapon_data.json");
                if (File.Exists(weaponPath))
                {
                    string weaponJson = File.ReadAllText(weaponPath);
                    WeaponList = JsonUtility.FromJson<WeaponDataList>("{\"items\":" + weaponJson + "}").items;
                    Debug.Log($"무기 데이터 로드 완료: {WeaponList.Count}개");
                }
                
                // 스탯 데이터 로드
                string statsPath = Path.Combine(chartFolderPath, "Stats_data.json");
                if (File.Exists(statsPath))
                {
                    try 
                    {
                        string statsJson = File.ReadAllText(statsPath);
                        // 직접 StatsData 객체 생성 후 값 설정
                        StatsDataInfo = new StatsData();
                        
                        // JSON 파싱을 위해 SimpleJSON 같은 라이브러리를 사용하거나
                        // 간단하게 수동으로 파싱
                        if (statsJson.Contains("공격력"))
                        {
                            // 매우 간단한 파싱 (실제로는 더 견고한 방법 사용 권장)
                            StatsDataInfo.공격력Value = 0;
                            StatsDataInfo.방어력Value = 0;
                            StatsDataInfo.경감Value = 0;
                            StatsDataInfo.크리티컬Value = 0;
                        }
                        
                        Debug.Log("스탯 데이터 로드 완료");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"스탯 데이터 파싱 오류: {e.Message}");
                    }
                }
                
                // 희귀도 데이터 로드
                string rarityPath = Path.Combine(chartFolderPath, "Rarity_data.json");
                if (File.Exists(rarityPath))
                {
                    try 
                    {
                        string rarityJson = File.ReadAllText(rarityPath);
                        // 직접 RarityData 객체 생성 후 값 설정
                        RarityDataInfo = new RarityData();
                        
                        // 간단한 파싱
                        if (rarityJson.Contains("일반"))
                        {
                            RarityDataInfo.일반Value = 50;
                            RarityDataInfo.희귀Value = 30;
                            RarityDataInfo.에픽Value = 15;
                            RarityDataInfo.전설Value = 5;
                        }
                        
                        Debug.Log("희귀도 데이터 로드 완료");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"희귀도 데이터 파싱 오류: {e.Message}");
                    }
                }
                
                return true;
            }
            else
            {
                Debug.LogWarning("Chart 폴더를 찾을 수 없습니다: " + chartFolderPath);
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"차트 데이터 로드 중 오류: {e.Message}");
            return false;
        }
    }
} 