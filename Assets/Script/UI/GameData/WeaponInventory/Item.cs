// Copyright 2013-2022 AFI, INC. All rights reserved.

namespace BackendData.GameData.WeaponInventory {
    //===============================================================
    // WeaponInventory 테이블의 Dictionary에 저장될 각 무기 정보 클래스
    //===============================================================
    public class Item {
        public string MyWeaponId { get; private set; }
        public int WeaponLevel { get; private set; } 
        public int WeaponChartId { get; private set; }

        public Item(string myWeaponId, int weaponLevel, int weaponChartId) {
            MyWeaponId = myWeaponId;
            WeaponLevel = weaponLevel;
            WeaponChartId = weaponChartId;
        }

        public void LevelUp() {
            WeaponLevel++;
        }
    }
}