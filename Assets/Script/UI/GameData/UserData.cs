// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BackendData.GameData {
    //===============================================================
    // UserData 테이블의 데이터를 담당하는 클래스(변수)
    //===============================================================
    public partial class UserData {
        public int Level { get; private set; }
        public float MaxExp { get; private set; }
        public float CurrentExp { get; private set; }
        public float Attack { get; private set; }
        public float Defense { get; private set; }
        public float DefenseRate { get; private set; }
        public float CriticalRate { get; private set; }
        public float Gold { get; private set; }
        public string LastLoginTime { get; private set; }
    }

    //===============================================================
    // UserData 테이블의 데이터를 담당하는 클래스(함수)
    //===============================================================
    public partial class UserData : Base.GameData {
        
        // 데이터가 존재하지 않을 경우, 초기값 설정
        protected override void InitializeData() {
            // 레벨
            Level = 1;
            // 최대 경험치
            MaxExp = 100;
            // 현재 경험치
            CurrentExp = 0;
            // 공격력
            Attack = 150;
            // 방어력
            Defense = 100;
            // 방어율
            DefenseRate = 0;
            // 크리티컬율
            CriticalRate = 0;
            // 보유 골드
            Gold = 10000;
            // 마지막 로그인 시간
            LastLoginTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        // 테이블 이름 설정 함수
        public override string GetFileName() {
            return "UserData";
        }

        // 적 처치 횟수를 갱신하는 함수
        public void CountDefeatEnemy() {
            // DayDefeatEnemyCount++;
            // WeekDefeatEnemyCount++;
            // MonthDefeatEnemyCount++;
            IsChangedData = true;
        }

        // 유저의 정보를 변경하는 함수
        public void UpdateUserData(float gold, float exp) {
            IsChangedData = true;
            
            CurrentExp += exp;
            Gold += gold;

            if (CurrentExp > MaxExp) {
                while (CurrentExp > MaxExp) {
                    LevelUp();
                }
            }
        }

        // 레벨업하는 함수
        private void LevelUp() {
            //CurrentExp가 MaxExp를 초과했을 경우를 대비하여 빼기
            CurrentExp -= MaxExp;

            //기존 경험치에서 1.1배
            MaxExp = (float)Math.Truncate(MaxExp * 1.1);

            Level++;
            
            // 레벨업 시 스탯 증가
            Attack += 10;
            Defense += 5;
            DefenseRate += 0.5f;
            CriticalRate += 0.5f;
        }

        public void UpdateLoginTime(DateTime time) {
            LastLoginTime = time.ToString(CultureInfo.InvariantCulture);
            IsChangedData = true;
        }

        protected override Dictionary<string, object> GetSaveData() {
            return new Dictionary<string, object> {
                {"Level", Level},
                {"MaxExp", MaxExp},
                {"CurrentExp", CurrentExp},
                {"Attack", Attack},
                {"Defense", Defense},
                {"DefenseRate", DefenseRate},
                {"CriticalRate", CriticalRate},
                {"Gold", Gold},
                {"LastLoginTime", LastLoginTime}
            };
        }

        protected override Dictionary<string, object> GetJsonData()
        {
            // GetSaveData() 메서드를 사용하도록 변경
            return GetSaveData();
        }

        protected override void LoadFromJson(Dictionary<string, object> json)
        {
            try
            {
                if (json.TryGetValue("Level", out object levelObj))
                    Level = Convert.ToInt32(levelObj);
                if (json.TryGetValue("MaxExp", out object maxExpObj))
                    MaxExp = Convert.ToSingle(maxExpObj);
                if (json.TryGetValue("CurrentExp", out object currentExpObj))
                    CurrentExp = Convert.ToSingle(currentExpObj);
                if (json.TryGetValue("Attack", out object attackObj))
                    Attack = Convert.ToSingle(attackObj);
                if (json.TryGetValue("Defense", out object defenseObj))
                    Defense = Convert.ToSingle(defenseObj);
                if (json.TryGetValue("DefenseRate", out object defenseRateObj))
                    DefenseRate = Convert.ToSingle(defenseRateObj);
                if (json.TryGetValue("CriticalRate", out object criticalRateObj))
                    CriticalRate = Convert.ToSingle(criticalRateObj);
                if (json.TryGetValue("Gold", out object goldObj))
                    Gold = Convert.ToSingle(goldObj);
                if (json.TryGetValue("LastLoginTime", out object lastLoginTimeObj))
                    LastLoginTime = Convert.ToString(lastLoginTimeObj);

                IsChangedData = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"UserData 로드 중 오류: {e.Message}");
                InitializeData();
            }
        }
    }
}