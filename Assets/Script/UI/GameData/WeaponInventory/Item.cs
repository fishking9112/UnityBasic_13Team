// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;

namespace BackendData.GameData.WeaponInventory {
    //===============================================================
    // WeaponInventory 테이블의 Dictionary에 저장될 각 무기 정보 클래스
    //===============================================================
    [Serializable]
    public class Item {
        // 기본 속성
        public int WeaponId { get; set; }
        public int Type { get; set; }
        public int Rating { get; set; }
        public string Name { get; set; }
        public float CapabilityValue { get; set; }

        // 생성자
        public Item(int weaponId, int type, int rating) {
            WeaponId = weaponId;
            Type = type;
            Rating = rating;
        }

        // 추가 생성자
        public Item(int weaponId, int type, int rating, string name, float capabilityValue)
            : this(weaponId, type, rating) {
            Name = name;
            CapabilityValue = capabilityValue;
        }

        // 문자열 표현
        public override string ToString() {
            return $"Item(ID: {WeaponId}, Type: {Type}, Rating: {Rating}, Name: {Name})";
        }
    }
}