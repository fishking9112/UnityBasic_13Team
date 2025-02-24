// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackendData.GameData.WeaponEquip {

    //===============================================================
    // WeaponEquip 테이블의 데이터를 관리하는 클래스
    //===============================================================
    public class Manager : Base.GameData {
        
        // WeaponEquip 각 아이템을 담는 Dictionary
        private Dictionary<string, int> _dictionary = new ();
        
        // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
        public IReadOnlyDictionary<string, int> Dictionary => _dictionary;
        
        // 테이블 이름 설정 함수
        public override string GetFileName() {
            return "WeaponEquip";
        }

        // 데이터가 존재하지 않을 경우, 초기값 설정
        protected override void InitializeData() {
            _dictionary.Clear();
        }
        
        // 데이터 저장 시 저장할 데이터를 뒤끝에 맞게 파싱하는 함수
        protected override Dictionary<string, object> GetSaveData() {
            var saveData = new Dictionary<string, object>();
            foreach (var item in _dictionary) {
                saveData[item.Key] = item.Value;
            }
            return saveData;
        }
        
        // Backend.GameData.GetMyData 호출 이후 리턴된 값을 파싱하여 캐싱하는 함수
        // 서버에서 데이터를 불러오늖 함수는 BackendData.Base.GameData의 BackendGameDataLoad() 함수를 참고해주세요
        protected override void LoadFromJson(Dictionary<string, object> json) {
            _dictionary.Clear();
            foreach (var item in json) {
                string weaponId = item.Key;
                int position = Convert.ToInt32(item.Value);
                _dictionary.Add(weaponId, position);
            }
        }

        protected override Dictionary<string, object> GetJsonData() {
            return GetSaveData();
        }

        // 장착된 무기에 대한 정보를 변경하는 함수
        public void ChangeEquip(int position, string prevWeaponId, string myWeaponId) {
            IsChangedData = true;
            if (!string.IsNullOrEmpty(prevWeaponId)) {
                _dictionary.Remove(prevWeaponId);
            }
            if (_dictionary.ContainsKey(myWeaponId)) {
                _dictionary.Remove(myWeaponId);
            }
            _dictionary.Add(myWeaponId, position);
        }
    }
}
