using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

/// <summary>
/// 유저의 골드 정보를 UI에 표시하는 간단한 스크립트
/// </summary>
public class GoldDisplayManager : MonoBehaviour
{
    [SerializeField] private Text goldText;
    [SerializeField] private float updateInterval = 2.0f; // 업데이트 간격 (초)
    
    private int currentGold = 0;
    private float lastUpdateTime = 0f;
    
    // GameDataManager 참조
    private GameDataManager gameDataManager;
    
    private void Start()
    {
        // GameDataManager 참조 가져오기
        gameDataManager = GameDataManager.Instance;
        
        // 초기 골드 로드
        LoadGoldData();
    }
    
    private void Update()
    {
        // 주기적으로 골드 데이터 업데이트
        if (Time.time - lastUpdateTime > updateInterval)
        {
            LoadGoldData();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// 골드 정보 로드
    /// </summary>
    private void LoadGoldData()
    {
        // GameDataManager 사용 시도
        if (gameDataManager != null)
        {
            try
            {
                // GameDataManager에서 골드 정보 가져오기 (리플렉션 사용)
                System.Type type = gameDataManager.GetType();
                System.Reflection.FieldInfo field = type.GetField("userData", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Public);
                
                if (field != null)
                {
                    object userData = field.GetValue(gameDataManager);
                    if (userData != null)
                    {
                        System.Type userDataType = userData.GetType();
                        System.Reflection.PropertyInfo goldProperty = userDataType.GetProperty("Gold");
                        
                        if (goldProperty != null)
                        {
                            // 안전한 형변환을 위해 object로 먼저 받고 Convert 클래스 사용
                            object goldValue = goldProperty.GetValue(userData);
                            
                            if (goldValue != null)
                            {
                                // 다양한 형변환 시도
                                try
                                {
                                    currentGold = Convert.ToInt32(goldValue);
                                }
                                catch
                                {
                                    // int로 직접 변환이 안되면 string으로 변환 후 int로 파싱 시도
                                    string goldStr = goldValue.ToString();
                                    if (int.TryParse(goldStr, out int parsedGold))
                                    {
                                        currentGold = parsedGold;
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"골드 값을 정수로 변환할 수 없습니다: {goldStr}");
                                        // 기본값 사용
                                        currentGold = 0;
                                    }
                                }
                                
                                // UI 업데이트
                                UpdateGoldUI();
                                return; // 성공했으면 여기서 종료
                            }
                        }
                    }
                }
                
                Debug.LogWarning("GameDataManager에서 userData 또는 Gold 속성을 찾을 수 없습니다.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GameDataManager에서 골드 정보를 가져오는 중 오류 발생: {ex.Message}");
                Debug.LogError($"스택 트레이스: {ex.StackTrace}");
            }
        }
        else
        {
            Debug.LogWarning("GameDataManager를 찾을 수 없습니다.");
        }
        
        // 위 방법이 실패하면 직접 파일에서 로드 시도
        LoadGoldFromFile();
    }
    
    /// <summary>
    /// 파일에서 직접 골드 정보 로드 (대체 방법)
    /// </summary>
    private void LoadGoldFromFile()
    {
        try
        {
            // 유저 데이터 파일 경로
            string userDataPath = Path.Combine(Application.persistentDataPath, "UserData.json");
            
            if (File.Exists(userDataPath))
            {
                string jsonData = File.ReadAllText(userDataPath);
                Debug.Log($"UserData.json 내용: {jsonData}");
                
                // JSON 파싱하여 골드 값만 추출
                JSONObject jsonObj = new JSONObject(jsonData);
                if (jsonObj.HasField("Gold"))
                {
                    currentGold = (int)jsonObj.GetField("Gold").i;
                    Debug.Log($"파일에서 로드한 골드: {currentGold}");
                    
                    // UI 업데이트
                    UpdateGoldUI();
                }
                else
                {
                    Debug.LogWarning("UserData.json에 Gold 필드가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("UserData.json 파일을 찾을 수 없습니다: " + userDataPath);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("골드 데이터 로드 중 오류 발생: " + ex.Message);
        }
    }
    
    /// <summary>
    /// 골드 UI 업데이트
    /// </summary>
    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = string.Format("{0:#,0}", currentGold);
            Debug.Log($"골드 UI 업데이트: {currentGold}");
        }
    }
}

/// <summary>
/// JSON 파싱을 위한 간단한 클래스
/// </summary>
public class JSONObject
{
    private Dictionary<string, JSONValue> values = new Dictionary<string, JSONValue>();
    
    public JSONObject(string json)
    {
        // 매우 간단한 JSON 파서 (실제로는 JsonUtility나 다른 라이브러리 사용 권장)
        json = json.Trim();
        if (json.StartsWith("{") && json.EndsWith("}"))
        {
            json = json.Substring(1, json.Length - 2);
            string[] pairs = json.Split(',');
            
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split(':');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim().Trim('"');
                    string value = keyValue[1].Trim();
                    
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        // 문자열 값
                        values[key] = new JSONValue { s = value.Trim('"') };
                    }
                    else if (value == "true" || value == "false")
                    {
                        // 불리언 값
                        values[key] = new JSONValue { b = value == "true" };
                    }
                    else
                    {
                        // 숫자 값으로 가정
                        if (int.TryParse(value, out int intValue))
                        {
                            values[key] = new JSONValue { i = intValue };
                        }
                        else if (float.TryParse(value, out float floatValue))
                        {
                            values[key] = new JSONValue { f = floatValue };
                        }
                    }
                }
            }
        }
    }
    
    public bool HasField(string key)
    {
        return values.ContainsKey(key);
    }
    
    public JSONValue GetField(string key)
    {
        if (values.TryGetValue(key, out JSONValue value))
        {
            return value;
        }
        return new JSONValue();
    }
}

/// <summary>
/// JSON 값을 저장하는 구조체
/// </summary>
public struct JSONValue
{
    public string s;  // 문자열
    public int i;     // 정수
    public float f;   // 부동소수점
    public bool b;    // 불리언
} 