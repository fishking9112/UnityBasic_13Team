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

        public string GetId() {
            return _id;
        }

        protected bool IsChangedData;

        // 파일 관련 추상 메서드
        public abstract string GetFileName();  // JSON 파일 이름 (예: "UserData", "WeaponInventory" 등)
        protected abstract void InitializeData();  // 초기 데이터 설정
        protected abstract Dictionary<string, object> GetSaveData();  // 저장할 데이터 반환
        protected abstract void LoadFromJson(Dictionary<string, object> json);  // JSON 데이터 로드

        // 데이터를 JSON으로 변환
        protected abstract Dictionary<string, object> GetJsonData();

        // 데이터 초기화 및 저장
        public void CreateNewData() {
            try {
                InitializeData();  // 각 매니저의 초기값으로 데이터 설정
                SaveToJson();      // JSON 파일로 저장
                IsChangedData = false;
            }
            catch (Exception e) {
                Debug.LogError($"Failed to create {GetFileName()}: {e}");
            }
        }

        // JSON 파일로 저장
        public void SaveToJson() {
            if (!IsChangedData) return;

            string path = GetJsonPath();
            if (string.IsNullOrEmpty(path)) return;

            try {
                var jsonData = GetJsonData();
                // Dictionary를 SerializableDict로 변환
                var serializableData = new SerializableDict<string, object>(jsonData);
                // 들여쓰기 없이 저장 (true -> false)
                string jsonString = JsonUtility.ToJson(serializableData);
                File.WriteAllText(path, jsonString);
                IsChangedData = false;
                Debug.Log($"{GetFileName()} 데이터가 저장되었습니다: {path}");
            }
            catch (Exception e) {
                Debug.LogError($"{GetFileName()} 데이터 저장 중 오류 발생: {e.Message}");
            }
        }

        // JSON 파일에서 로드
        public bool LoadFromJson() {
            string path = GetJsonPath();
            if (string.IsNullOrEmpty(path)) return false;

            try {
                if (!File.Exists(path)) {
                    Debug.Log($"{GetFileName()} 데이터 파일이 없습니다: {path}");
                    return false;
                }

                string jsonString = File.ReadAllText(path);
                // SerializableDict로 변환 후 Dictionary로 변환
                var serializableData = JsonUtility.FromJson<SerializableDict<string, object>>(jsonString);
                var data = serializableData.ToDictionary();
                LoadFromJson(data);
                Debug.Log($"{GetFileName()} 데이터를 로드했습니다: {path}");
                return true;
            }
            catch (Exception e) {
                Debug.LogError($"{GetFileName()} 데이터 로드 중 오류 발생: {e.Message}");
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
            Debug.Log($"JSON 파일 경로: {path}");
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