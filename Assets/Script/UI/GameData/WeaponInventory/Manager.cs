// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackendData.GameData.WeaponInventory {

    //===============================================================
    // WeaponInventory 테이블의 데이터를 관리하는 클래스
    //===============================================================
    public class Manager : Base.GameData {
        
        // WeaponInventory의 각 아이템을 담는 Dictionary
        private Dictionary<string, Item> _dictionary = new ();

        // 다른 클래스에서 Add, Delete등 수정이 불가능하도록 읽기 전용 Dictionary
        public IReadOnlyDictionary<string, Item> Dictionary => _dictionary;

        // 테이블 이름 설정 함수
        public override string GetFileName() {
            return "WeaponInventory";
        }

        // 데이터가 존재하지 않을 경우, 초기값 설정
        protected override void InitializeData() {
            _dictionary.Clear();
            AddWeapon(1);        
        }
        
        // 데이터 저장 시 저장할 데이터를 뒤끝에 맞게 파싱하는 함수
        protected override Dictionary<string, object> GetSaveData() {
            var saveData = new Dictionary<string, object>();
            foreach (var item in _dictionary) {
                saveData[item.Key] = new Dictionary<string, object> {
                    {"WeaponLevel", item.Value.WeaponLevel},
                    {"MyWeaponId", item.Value.MyWeaponId},
                    {"WeaponChartId", item.Value.WeaponChartId}
                };
            }
            return saveData;
        }
        
        // Backend.GameData.GetMyData 호출 이후 리턴된 값을 파싱하여 캐싱하는 함수
        // 서버에서 데이터를 불러오늖 함수는 BackendData.Base.GameData의 BackendGameDataLoad() 함수를 참고해주세요
        protected override void LoadFromJson(Dictionary<string, object> json) {
            _dictionary.Clear();
            foreach (var item in json) {
                var data = (Dictionary<string, object>)item.Value;
                int weaponLevel = Convert.ToInt32(data["WeaponLevel"]);
                string myWeaponId = data["MyWeaponId"].ToString();
                int weaponChartId = Convert.ToInt32(data["WeaponChartId"]);
                
                _dictionary.Add(myWeaponId, new Item(myWeaponId, weaponLevel, weaponChartId));
            }
        }
        
        // 인벤토리에 무기 추가
        public string AddWeapon(int weaponId) {
            IsChangedData = true;
            
            // 무기의 고유 아이디 생성
            string myWeaponID = DateTime.Now.Ticks.ToString();
            _dictionary.Add(myWeaponID, new Item(myWeaponID, 1, weaponId));

            return myWeaponID;
        }
        
        protected override Dictionary<string, object> GetJsonData() {
            return GetSaveData();
        }
    }
}