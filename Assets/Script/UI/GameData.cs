// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace BackendData.Base {
    //===============================================================
    // 게임 데이터의 불러오기와 쓰기에 대한 공통적인 로직을 가진 클래스
    //===============================================================
    public abstract class GameData {
        private string _id;  // 데이터 식별자
        private string _filePath; // 파일 경로 저장 변수 추가

        public string GetId() {
            return _id;
        }

        protected bool IsChangedData;

        // 상세 로그 출력 여부를 제어하는 정적 속성
        public static bool EnableDetailedLogs = true;

        // 파일 관련 추상 메서드
        public abstract string GetFileName();  // JSON 파일 이름 (예: "UserData", "WeaponInventory" 등)
        protected abstract void InitializeData();  // 초기 데이터 설정
        protected abstract Dictionary<string, object> GetSaveData();  // 저장할 데이터 반환
        protected abstract void LoadFromJson(Dictionary<string, object> json);  // JSON 데이터 로드

        // 데이터를 JSON으로 변환
        protected abstract Dictionary<string, object> GetJsonData();

        // Dictionary를 JSON 문자열로 변환하는 메서드
        protected string ConvertToJsonString(Dictionary<string, object> data)
        {
            var serializableData = new SerializableDict<string, object>(data);
            return JsonUtility.ToJson(serializableData, true);
        }

        // 데이터 초기화 및 저장
        public virtual void CreateNewData()
        {
            try
            {
                // 데이터 초기화
                InitializeData();
                
                // 변경 플래그 설정
                IsChangedData = true;
                
                // 즉시 저장
                SaveToJson();
                
                Debug.Log($"{GetFileName()} 새 데이터가 생성되었습니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetFileName()} 데이터 생성 중 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // JSON 파일로 저장
        public virtual void SaveToJson()
        {
            try
            {
                // 파일 경로 설정
                string filePath = Path.Combine(Application.persistentDataPath, $"{GetFileName()}.json");
                _filePath = filePath; // 필드에 경로 저장
                
                Debug.Log($"JSON 파일 경로: {filePath}");
                
                // 데이터 변환
                Dictionary<string, object> saveData = GetSaveData();
                string jsonString = ConvertToJsonString(saveData);
                
                // 디렉토리 확인 및 생성
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 파일 저장
                File.WriteAllText(filePath, jsonString);
                Debug.Log($"{GetFileName()} 데이터가 저장되었습니다: {filePath}");
                
                // 저장 후 파일 존재 확인
                if (File.Exists(filePath))
                {
                    Debug.Log($"{GetFileName()} 파일이 성공적으로 생성되었습니다.");
                    
                    // 파일 내용 확인 (디버깅용)
                    string content = File.ReadAllText(filePath);
                    Debug.Log($"{GetFileName()} 파일 내용: {content}");
                }
                else
                {
                    Debug.LogError($"{GetFileName()} 파일 생성 실패: {filePath}");
                }
                
                IsChangedData = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{GetFileName()} 데이터 저장 중 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // JSON 파일에서 로드
        public bool LoadFromJson() {
            try {
                string filePath = GetJsonPath();
                
                if (EnableDetailedLogs)
                    Debug.Log($"JSON 파일 경로: {filePath}");
                
                if (!File.Exists(filePath)) {
                    if (EnableDetailedLogs)
                        Debug.Log($"{GetFileName()} 데이터 파일이 없습니다: {filePath}");
                    return false;
                }

                string jsonString = File.ReadAllText(filePath);
                // SerializableDict로 변환 후 Dictionary로 변환
                var serializableData = JsonUtility.FromJson<SerializableDict<string, object>>(jsonString);
                var data = serializableData.ToDictionary();
                LoadFromJson(data);
                
                if (EnableDetailedLogs)
                    Debug.Log($"{GetFileName()} 데이터를 로드했습니다: {filePath}");
                
                return true;
            }
            catch (Exception e) {
                Debug.LogError($"{GetFileName()} 로드 중 오류: {e.Message}");
                return false;
            }
        }

        protected virtual string GetJsonPath() {
            // 각 데이터 타입별로 고유한 파일 경로 생성
            string fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName)) {
                Debug.LogError("파일 이름이 지정되지 않았습니다.");
                return null;
            }

            // 경로 생성 및 디버그 로그 추가
            string path = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
            return path;
        }

        public void ForceSetChanged()
        {
            IsChangedData = true;
        }
    }

    // JSON 직렬화를 위한 헬퍼 클래스
    [Serializable]
    public class SerializableDict<TKey, TValue>
    {
        public List<string> keys;
        public List<string> values;

        public SerializableDict()
        {
            keys = new List<string>();
            values = new List<string>();
        }

        public SerializableDict(Dictionary<TKey, TValue> dict)
        {
            keys = new List<string>();
            values = new List<string>();
            
            if (dict != null)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in dict)
                {
                    keys.Add(kvp.Key.ToString());
                    // 값을 직접 문자열로 변환
                    values.Add(kvp.Value?.ToString() ?? "");
                }
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
            
            int count = Mathf.Min(keys.Count, values.Count);
            
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(keys[i]))
                {
                    try
                    {
                        TKey key = (TKey)Convert.ChangeType(keys[i], typeof(TKey));
                        TValue value = (TValue)Convert.ChangeType(values[i], typeof(TValue));
                        dict[key] = value;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"값 변환 중 오류: {e.Message}, Key: {keys[i]}, Value: {values[i]}");
                    }
                }
            }
            
            return dict;
        }
    }
}