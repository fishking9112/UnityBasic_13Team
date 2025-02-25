// Copyright 2013-2022 AFI, INC. All rights reserved.


using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using BackendData.GameData;
// using TMPro;  // TMP_Text를 위해 필요

namespace UI {
    public class LoadingSceneManager : MonoBehaviour {

        [SerializeField] private Text loadingText;
        [SerializeField] private Slider loadingSlider;

        private int _maxLoadingCount; // 총 뒤끝 함수를 호출할 갯수

        private int _currentLoadingCount; // 현재 뒤끝 함수를 호출한 갯수
        
        private readonly Queue<Action> _initializeStep = new Queue<Action>();

        void Awake() {
            // Awake에서 Init() 제거
        }
        
        void Start() {
            Init();
            if (_initializeStep.Count > 0) {
                _initializeStep.Dequeue()?.Invoke();
            }
        }

        void Init() {
            _initializeStep.Clear();
            
            // 게임 데이터 초기화 수행
            _initializeStep.Enqueue(() => { 
                ShowDataName("게임 데이터");
                InitializeGameData();
            });
            
            // 차트 데이터 로드 추가
            _initializeStep.Enqueue(() => {
                ShowDataName("차트 데이터");
                LoadChartData(NextStep);
            });

            // 로딩 UI 초기화
            _maxLoadingCount = _initializeStep.Count;
            loadingSlider.maxValue = _maxLoadingCount;
            _currentLoadingCount = 0;
            loadingSlider.value = _currentLoadingCount;
        }

        private void ShowDataName(string text) {
            string info = $"{text} 불러오는 중...({_currentLoadingCount}/{_maxLoadingCount})";
            loadingText.text = info;
        }

        private void NextStep(bool isSuccess, string className, string funcName, string errorInfo) {
            if (isSuccess) {
                _currentLoadingCount++;
                loadingSlider.value = _currentLoadingCount;

                if (_initializeStep.Count > 0) {
                    _initializeStep.Dequeue()?.Invoke();
                }
                else {
                    InGameStart();
                }
            }
            else {
                Debug.LogError($"Error: {className} - {funcName}: {errorInfo}");
            }
        }

        // 인게임씬으로 이동가는 함수
        private void InGameStart() {
            loadingText.text = "게임 시작하는 중";
            _initializeStep.Clear();
            SceneManager.LoadScene("Main_scene");
        }

        private void InitializeGameData()
        {
            ShowDataName("게임 데이터");
            
            // 게임 데이터 매니저 생성
            var gameDataObj = new GameObject("GameDataManager");
            var gameDataManager = gameDataObj.AddComponent<GameDataManager>();
            DontDestroyOnLoad(gameDataObj);

            try 
            {
                // UserData를 체크하여 신규/기존 유저 구분
                var userData = new UserData();
                bool isNewUser = !userData.LoadFromJson();

                if (isNewUser)
                {
                    Debug.Log("신규 유저 - 새 데이터 생성");
                    gameDataManager.CreateNewUserData();
                    _currentLoadingCount++;
                    loadingSlider.value = _currentLoadingCount;
                }
                else
                {
                    Debug.Log("기존 유저 - 데이터 로드");
                    gameDataManager.LoadExistingUserData();
                    _currentLoadingCount++;
                    loadingSlider.value = _currentLoadingCount;
                }

                // 다음 초기화 단계로 진행
                NextStep(true, "", "", "");
            }
            catch (Exception e)
            {
                Debug.LogError($"게임 데이터 초기화 실패: {e.Message}");
                NextStep(false, "GameData", "InitializeGameData", e.Message);
            }
        }

        // 차트 데이터 로드 함수
        private void LoadChartData(Action<bool, string, string, string> callback)
        {
            // ChartManager 생성
            var chartManagerObj = new GameObject("ChartManager");
            var chartManagerType = Type.GetType("ChartManager");
            
            if (chartManagerType != null)
            {
                var chartManager = chartManagerObj.AddComponent(chartManagerType) as MonoBehaviour;
                DontDestroyOnLoad(chartManagerObj);
                
                // 리플렉션으로 메서드 호출
                var loadMethod = chartManagerType.GetMethod("LoadAllChartData");
                bool success = (bool)loadMethod.Invoke(chartManager, null);
                
                if (success)
                {
                    Debug.Log("모든 차트 데이터 로드 완료");
                    callback(true, "", "", "");
                }
                else
                {
                    Debug.LogError("차트 데이터 로드 실패");
                    callback(false, "LoadingSceneManager", "LoadChartData", "차트 데이터 로드 실패");
                }
            }
            else
            {
                Debug.LogError("ChartManager 타입을 찾을 수 없습니다");
                callback(false, "LoadingSceneManager", "LoadChartData", "ChartManager 타입을 찾을 수 없습니다");
            }
        }
    }
}