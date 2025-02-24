// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackendData.GameData.QuestAchievement {
    
    //===============================================================
    // WeaponInventory 테이블의 데이터를 관리하는 클래스
    //===============================================================
    public class Manager : Base.GameData {
        
        // QuestAchievement의 각 아이템을 담는 Dictionary
        private Dictionary<int, Item> _dictionary = new();

        // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
        public IReadOnlyDictionary<int, Item> Dictionary => _dictionary;

        // 테이블 이름 설정 함수
        public override string GetFileName() {
            return "QuestAchievement";
        }
        
        // 데이터가 존재하지 않을 경우, 초기값 설정
        protected override void InitializeData() {
            _dictionary.Clear();
            // 초기 퀘스트 데이터 설정
            for (int i = 1; i <= 10; i++) {  // 예시로 10개의 퀘스트 생성
                _dictionary.Add(i, new Item(i, false));
            }
        }
        
        protected override Dictionary<string, object> GetSaveData() {
            var saveData = new Dictionary<string, object>();
            foreach (var item in _dictionary) {
                saveData[item.Key.ToString()] = new Dictionary<string, object> {
                    {"QuestId", item.Value.QuestId},
                    {"IsAchieve", item.Value.IsAchieve}
                };
            }
            return saveData;
        }

        protected override void LoadFromJson(Dictionary<string, object> json) {
            _dictionary.Clear();
            foreach (var item in json) {
                var data = (Dictionary<string, object>)item.Value;
                int questId = Convert.ToInt32(data["QuestId"]);
                bool isAchieve = Convert.ToBoolean(data["IsAchieve"]);
                _dictionary.Add(questId, new Item(questId, isAchieve));
            }
        }

        protected override Dictionary<string, object> GetJsonData() {
            return GetSaveData();
        }

        // 특정 퀘스트를 달성처리하는 함수
        public void SetAchieve(int questId) {
            IsChangedData = true;
            _dictionary[questId].SetAchieve();
        }
    }
}