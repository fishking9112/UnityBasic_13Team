// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BackendData.Base;

namespace BackendData.GameData.WeaponInventory {

    //===============================================================
    // WeaponInventory 테이블의 데이터를 관리하는 클래스
    //===============================================================
    public class Manager : Base.GameData {
        
        // 무기 아이템 클래스 정의
        [Serializable]
        public class WeaponItem
        {
            public int id;
            public int Equip; // 0: 미장착, 1: 장착
            
            public WeaponItem(int id, int equip = 0)
            {
                this.id = id;
                this.Equip = equip;
            }
        }
        
        // 무기 아이템 리스트
        private List<WeaponItem> weaponItems = new List<WeaponItem>();
        private string fileName = "WeaponInventory";
        
        // 생성자
        public Manager() 
        {
            // 초기화 시 빈 리스트로 설정
            weaponItems = new List<WeaponItem>();
        }
        
        // 무기 추가 메서드
        public void AddWeapon(int weaponId)
        {
            // 새 무기 아이템 생성 (기본적으로 미장착 상태)
            var newItem = new WeaponItem(weaponId);
            weaponItems.Add(newItem);
            IsChangedData = true;
        }
        
        // 무기 제거 메서드
        public void RemoveWeapon(int weaponId)
        {
            for (int i = 0; i < weaponItems.Count; i++)
            {
                if (weaponItems[i].id == weaponId)
                {
                    weaponItems.RemoveAt(i);
                    IsChangedData = true;
                    break;
                }
            }
        }
        
        // 무기 장착 상태 변경 메서드
        public void SetEquipStatus(int weaponId, int equipStatus)
        {
            for (int i = 0; i < weaponItems.Count; i++)
            {
                if (weaponItems[i].id == weaponId)
                {
                    weaponItems[i].Equip = equipStatus;
                    IsChangedData = true;
                    break;
                }
            }
        }
        
        // 무기 보유 여부 확인
        public bool HasWeapon(int weaponId)
        {
            for (int i = 0; i < weaponItems.Count; i++)
            {
                if (weaponItems[i].id == weaponId)
                {
                    return true;
                }
            }
            return false;
        }
        
        // 무기 장착 상태 확인
        public int GetEquipStatus(int weaponId)
        {
            for (int i = 0; i < weaponItems.Count; i++)
            {
                if (weaponItems[i].id == weaponId)
                {
                    return weaponItems[i].Equip;
                }
            }
            return -1; // 무기가 없는 경우
        }
        
        // 모든 무기 아이템 가져오기
        public List<WeaponItem> GetAllWeaponItems()
        {
            return new List<WeaponItem>(weaponItems);
        }
        
        // 모든 무기 ID 가져오기
        public List<int> GetAllWeaponIds()
        {
            List<int> ids = new List<int>();
            foreach (var item in weaponItems)
            {
                ids.Add(item.id);
            }
            return ids;
        }
        
        // 강제로 변경 플래그 설정 - new 키워드 추가
        public new void ForceSetChanged()
        {
            IsChangedData = true;
        }
        
        // GameData 추상 메서드 구현
        public override string GetFileName()
        {
            return fileName;
        }
        
        protected override Dictionary<string, object> GetJsonData()
        {
            // 직접 배열 형태의 JSON을 생성하여 단일 키에 저장
            var result = new Dictionary<string, object>();
            
            // 각 무기 아이템을 개별 항목으로 저장
            for (int i = 0; i < weaponItems.Count; i++)
            {
                var itemDict = new Dictionary<string, object>();
                itemDict["id"] = weaponItems[i].id;
                itemDict["Equip"] = weaponItems[i].Equip;
                
                result[i.ToString()] = itemDict;
            }
            
            return result;
        }
        
        // 기존 GameData 클래스의 메서드 오버라이드
        protected override void InitializeData()
        {
            weaponItems.Clear();
        }
        
        protected override Dictionary<string, object> GetSaveData()
        {
            // 이 메서드는 사용하지 않지만 오버라이드해야 함
            return new Dictionary<string, object>();
        }
        
        protected override void LoadFromJson(Dictionary<string, object> json)
        {
            // 이 메서드는 사용하지 않지만 오버라이드해야 함
        }
        
        // 커스텀 저장 메서드
        public void CustomSaveToJson()
        {
            try
            {
                // 배열 형태로 직접 JSON 생성
                string json = "[";
                
                for (int i = 0; i < weaponItems.Count; i++)
                {
                    if (i > 0) json += ",";
                    json += $"{{\"id\":{weaponItems[i].id},\"Equip\":{weaponItems[i].Equip}}}";
                }
                
                json += "]";
                
                // 디버그: 저장할 내용 확인
                Debug.Log($"저장할 무기 아이템 목록: {json}");
                
                // 파일에 저장
                string path = Path.Combine(Application.persistentDataPath, GetFileName() + ".json");
                File.WriteAllText(path, json);
                
                Debug.Log($"WeaponInventory 데이터가 저장되었습니다: {path}, 내용: {json}");
                
                // 변경 플래그 초기화
                IsChangedData = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"WeaponInventory 저장 중 오류: {ex.Message}");
            }
        }
        
        // 커스텀 로드 메서드
        public void CustomLoadFromJson()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, GetFileName() + ".json");
                
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    
                    // 배열 형태 파싱
                    if (json.StartsWith("[") && json.EndsWith("]"))
                    {
                        weaponItems.Clear();
                        
                        // 간단한 JSON 파싱 (실제로는 JsonUtility 또는 다른 JSON 라이브러리 사용 권장)
                        json = json.Trim('[', ']');
                        if (!string.IsNullOrEmpty(json))
                        {
                            // 객체 단위로 분리
                            List<string> itemJsons = SplitJsonObjects(json);
                            
                            foreach (string itemJson in itemJsons)
                            {
                                try
                                {
                                    // id와 Equip 값 추출
                                    int id = ExtractIntValue(itemJson, "id");
                                    int equip = ExtractIntValue(itemJson, "Equip");
                                    
                                    // 무기 아이템 추가
                                    weaponItems.Add(new WeaponItem(id, equip));
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"아이템 파싱 중 오류: {ex.Message}");
                                }
                            }
                        }
                    }
                    // 기존 배열 형태 파싱 (마이그레이션)
                    else if (json.StartsWith("[") && !json.Contains("{"))
                    {
                        weaponItems.Clear();
                        
                        json = json.Trim('[', ']');
                        if (!string.IsNullOrEmpty(json))
                        {
                            string[] values = json.Split(',');
                            
                            foreach (string value in values)
                            {
                                if (string.IsNullOrEmpty(value)) continue;
                                
                                int weaponId;
                                if (int.TryParse(value.Trim(), out weaponId))
                                {
                                    // 기존 ID만 있는 형태에서 ID와 Equip(0)으로 변환
                                    weaponItems.Add(new WeaponItem(weaponId, 0));
                                }
                            }
                            
                            // 마이그레이션 후 새 형식으로 저장
                            CustomSaveToJson();
                        }
                    }
                    // 기존 키-값 형태 파싱 (마이그레이션)
                    else if (json.Contains("keys") && json.Contains("values"))
                    {
                        // 기존 형식에서 데이터 추출
                        try
                        {
                            int keysStart = json.IndexOf("[", json.IndexOf("keys")) + 1;
                            int keysEnd = json.IndexOf("]", keysStart);
                            int valuesStart = json.IndexOf("[", json.IndexOf("values")) + 1;
                            int valuesEnd = json.IndexOf("]", valuesStart);
                            
                            string valuesStr = json.Substring(valuesStart, valuesEnd - valuesStart);
                            string[] values = valuesStr.Split(',');
                            
                            weaponItems.Clear();
                            
                            foreach (string value in values)
                            {
                                if (string.IsNullOrEmpty(value)) continue;
                                
                                // Dictionary 형태에서 ID 추출 시도
                                if (value.Contains("System.Collections.Generic.Dictionary"))
                                {
                                    // 복잡한 형태이므로 무시
                                    continue;
                                }
                                
                                int weaponId;
                                if (int.TryParse(value.Trim('"', ' '), out weaponId))
                                {
                                    // 기존 ID만 있는 형태에서 ID와 Equip(0)으로 변환
                                    weaponItems.Add(new WeaponItem(weaponId, 0));
                                }
                            }
                            
                            // 마이그레이션 후 새 형식으로 저장
                            CustomSaveToJson();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"기존 데이터 변환 중 오류: {ex.Message}");
                            weaponItems.Clear();
                        }
                    }
                    
                    Debug.Log($"WeaponInventory 데이터 로드 완료: {weaponItems.Count}개 아이템");
                }
                else
                {
                    // 파일이 없으면 빈 배열로 초기화
                    weaponItems.Clear();
                    CustomSaveToJson();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"WeaponInventory 로드 중 오류: {ex.Message}");
                weaponItems.Clear();
            }
        }
        
        // JSON 객체 분리 헬퍼 메서드
        private List<string> SplitJsonObjects(string json)
        {
            List<string> result = new List<string>();
            int depth = 0;
            int startIndex = 0;
            
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                
                if (c == '{')
                {
                    if (depth == 0)
                    {
                        startIndex = i;
                    }
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        result.Add(json.Substring(startIndex, i - startIndex + 1));
                    }
                }
            }
            
            return result;
        }
        
        // JSON에서 정수 값 추출 헬퍼 메서드
        private int ExtractIntValue(string json, string key)
        {
            string pattern = $"\"{key}\":";
            int startIndex = json.IndexOf(pattern);
            if (startIndex < 0) return 0;
            
            startIndex += pattern.Length;
            int endIndex = json.IndexOf(",", startIndex);
            if (endIndex < 0) endIndex = json.IndexOf("}", startIndex);
            
            string valueStr = json.Substring(startIndex, endIndex - startIndex).Trim();
            int value;
            if (int.TryParse(valueStr, out value))
            {
                return value;
            }
            
            return 0;
        }

        // SaveToJson 메서드 오버라이드
        public new void SaveToJson()
        {
            // 커스텀 저장 메서드 호출
            CustomSaveToJson();
        }

        // LoadFromJson 메서드 오버라이드
        public new void LoadFromJson()
        {
            // 커스텀 로드 메서드 호출
            CustomLoadFromJson();
        }

        // 특정 아이템의 장착 상태 업데이트
        public void UpdateEquipStatus(int itemId, int itemIndex, bool isEquipped)
        {
            // 인덱스가 유효한지 확인
            if (itemIndex >= 0 && itemIndex < weaponItems.Count)
            {
                var item = weaponItems[itemIndex];
                
                // 아이템 ID가 일치하는지 확인
                if (item.id == itemId)
                {
                    // 같은 타입의 다른 아이템 장착 해제
                    if (isEquipped)
                    {
                        int itemType = GetItemType(itemId);
                        if (itemType > 0)
                        {
                            for (int i = 0; i < weaponItems.Count; i++)
                            {
                                if (i != itemIndex && GetItemType(weaponItems[i].id) == itemType && weaponItems[i].Equip == 1)
                                {
                                    weaponItems[i].Equip = 0;
                                }
                            }
                        }
                    }
                    
                    // 장착 상태 변경
                    item.Equip = isEquipped ? 1 : 0;
                    Debug.Log($"WeaponInventory 아이템 장착 상태 변경: ID {itemId}, 인덱스 {itemIndex}, 장착 {isEquipped}");
                    
                    // 변경 사항 저장
                    SaveToJson();
                }
                else
                {
                    Debug.LogWarning($"아이템 ID 불일치: 예상 {itemId}, 실제 {item.id}");
                }
            }
            else
            {
                Debug.LogError($"유효하지 않은 아이템 인덱스: {itemIndex}, 아이템 수: {weaponItems.Count}");
            }
        }

        // 아이템 타입 가져오기
        private int GetItemType(int itemId)
        {
            // 아이템 데이터에서 타입 정보 가져오기
            // 이 부분은 실제 구현에 맞게 수정 필요
            return 0; // 기본값
        }
    }
}