// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackendData.GameData.QuestAchievement {
    
    //===============================================================
    // QuestAchievement 테이블의 데이터를 관리하는 클래스
    //===============================================================
    public class Manager : Base.GameData {
        
        // QuestAchievement의 각 아이템을 담는 Dictionary
        private Dictionary<int, Item> _dictionary = new();
        
        // QuestData 클래스 정의
        [Serializable]
        public class QuestData {
            public int id;
            public string title;
            public string description;
            public bool isCompleted;
        }
        
        // 퀘스트 데이터 리스트
        private List<QuestData> _quests = new List<QuestData>();

        // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
        public IReadOnlyDictionary<int, Item> Dictionary => _dictionary;

        // 테이블 이름 설정 함수
        public override string GetFileName() {
            return "QuestAchievement";
        }
        
        // 데이터가 존재하지 않을 경우, 초기값 설정
        protected override void InitializeData() {
            // base.InitializeData() 호출 제거 (추상 메서드이므로 호출 불가)
            
            // 퀘스트 데이터 초기화
            _quests = new List<QuestData>();
            
            // 기본 퀘스트 추가 (예시)
            _quests.Add(new QuestData { id = 1, title = "첫 번째 퀘스트", description = "게임 시작하기", isCompleted = false });
            _quests.Add(new QuestData { id = 2, title = "두 번째 퀘스트", description = "무기 획득하기", isCompleted = false });
            
            Debug.Log($"QuestAchievement 초기화 완료: {_quests.Count}개의 퀘스트 생성");
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