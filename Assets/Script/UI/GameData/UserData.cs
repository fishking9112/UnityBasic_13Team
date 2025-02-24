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
        public float Money { get; private set; }
        public string LastLoginTime { get; private set; }
        public float Exp { get; private set; }
        public float MaxExp { get; private set; }
        public float Jewel { get; private set; }
        public float DayUsingGold { get; set; }
        public float WeekUsingGold { get; set; }
        public float MonthUsingGold { get; set; }
        public int DayDefeatEnemyCount { get; private set; }
        public int WeekDefeatEnemyCount { get; private set; }
        public int MonthDefeatEnemyCount { get; private set; }
    }

    //===============================================================
    // UserData 테이블의 데이터를 담당하는 클래스(함수)
    //===============================================================
    public partial class UserData : Base.GameData {
        
        // 데이터가 존재하지 않을 경우, 초기값 설정
        protected override void InitializeData() {
            // 초기 레벨
            Level = 1;
            // 초기 돈
            Money = 10000;
            // 최대 경험치
            MaxExp = 100;
            // 마지막 로그인 시간
            LastLoginTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            // 현재 경험치
            Exp = 0;
            // 보석
            Jewel = 0;
            // 하루 사용한 골드
            DayUsingGold = 0;
            // 일주일 사용한 골드
            WeekUsingGold = 0;
            // 한 달 사용한 골드
            MonthUsingGold = 0;
            // 하루 처치한 적 수
            DayDefeatEnemyCount = 0;
            // 일주일 처치한 적 수
            WeekDefeatEnemyCount = 0;
            // 한 달 처치한 적 수
            MonthDefeatEnemyCount = 0;
        }

        // 테이블 이름 설정 함수
        public override string GetFileName() {
            return "UserData";
        }

        // 적 처치 횟수를 갱신하는 함수
        public void CountDefeatEnemy() {
            DayDefeatEnemyCount++;
            WeekDefeatEnemyCount++;
            MonthDefeatEnemyCount++;
            IsChangedData = true;
        }

        // 유저의 정보를 변경하는 함수
        public void UpdateUserData(float money, float exp) {
            IsChangedData = true;
            
            Exp += exp;
            Money += money;

            if (money < 0) {
                float tempMoney = Math.Abs(money);
                DayUsingGold += tempMoney;
                WeekUsingGold += tempMoney;
                MonthUsingGold += tempMoney;
            }

            if (Exp > MaxExp) {
                while (Exp > MaxExp) {
                    LevelUp();
                }
            }
        }

        // 레벨업하는 함수
        private void LevelUp() {
            //Exp가 MaxExp를 초과했을 경우를 대비하여 빼기
            Exp -= MaxExp;

            //기존 경험치에서 1.1배
            MaxExp = (float)Math.Truncate(MaxExp * 1.1);

            Level++;
        }

        protected override Dictionary<string, object> GetSaveData() {
            return new Dictionary<string, object> {
                {"Level", Level},
                {"Money", Money},
                {"Exp", Exp},
                {"MaxExp", MaxExp},
                {"LastLoginTime", DateTime.Now.ToString(CultureInfo.InvariantCulture)},
                {"DayUsingGold", DayUsingGold},
                {"WeekUsingGold", WeekUsingGold},
                {"MonthUsingGold", MonthUsingGold},
                {"DayDefeatEnemyCount", DayDefeatEnemyCount},
                {"WeekDefeatEnemyCount", WeekDefeatEnemyCount},
                {"MonthDefeatEnemyCount", MonthDefeatEnemyCount},
                {"Jewel", Jewel}
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
                // TryGetValue를 사용하여 안전하게 데이터 로드
                if (json.TryGetValue("Level", out object levelObj))
                    Level = Convert.ToInt32(levelObj);
                if (json.TryGetValue("Money", out object moneyObj))
                    Money = Convert.ToSingle(moneyObj);
                if (json.TryGetValue("Exp", out object expObj))
                    Exp = Convert.ToSingle(expObj);
                if (json.TryGetValue("MaxExp", out object maxExpObj))
                    MaxExp = Convert.ToSingle(maxExpObj);
                if (json.TryGetValue("LastLoginTime", out object lastLoginObj))
                    LastLoginTime = lastLoginObj.ToString();
                if (json.TryGetValue("DayUsingGold", out object dayGoldObj))
                    DayUsingGold = Convert.ToSingle(dayGoldObj);
                if (json.TryGetValue("WeekUsingGold", out object weekGoldObj))
                    WeekUsingGold = Convert.ToSingle(weekGoldObj);
                if (json.TryGetValue("MonthUsingGold", out object monthGoldObj))
                    MonthUsingGold = Convert.ToSingle(monthGoldObj);
                if (json.TryGetValue("DayDefeatEnemyCount", out object dayEnemyObj))
                    DayDefeatEnemyCount = Convert.ToInt32(dayEnemyObj);
                if (json.TryGetValue("WeekDefeatEnemyCount", out object weekEnemyObj))
                    WeekDefeatEnemyCount = Convert.ToInt32(weekEnemyObj);
                if (json.TryGetValue("MonthDefeatEnemyCount", out object monthEnemyObj))
                    MonthDefeatEnemyCount = Convert.ToInt32(monthEnemyObj);
                if (json.TryGetValue("Jewel", out object jewelObj))
                    Jewel = Convert.ToSingle(jewelObj);

                IsChangedData = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"UserData 로드 중 오류: {e.Message}");
                // 오류 발생 시 초기값으로 설정
                InitializeData();
            }
        }
    }
}