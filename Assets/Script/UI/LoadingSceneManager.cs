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
            
            // 게임 데이터 초기화만 수행
            _initializeStep.Enqueue(() => { 
                ShowDataName("게임 데이터");
                InitializeGameData();
            });

            // 차트 데이터 로드 부분 제거

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

        // 로컬 JSON에서 차트 정보를 로드하는 함수
        private void LoadChartInfoLocally(Action<bool, string, string, string> func) {
            try {
                string chartDataJson = LoadLocalChartData();
                if (!string.IsNullOrEmpty(chartDataJson)) {
                    var chartData = JsonUtility.FromJson<ChartDataList>(chartDataJson);
                    foreach (var chartInfo in chartData.charts) {
                        _initializeStep.Enqueue(() => {
                            ShowDataName(chartInfo.name);
                            ProcessChartInfoLocally(chartInfo, NextStep);
                        });
                        _maxLoadingCount++;
                    }
                    loadingSlider.maxValue = _maxLoadingCount;
                    func.Invoke(true, "", "", "");
                } else {
                    func.Invoke(true, "", "", "");
                }
            } catch {
                func.Invoke(true, "", "", "");
            }
        }

        // 로컬 JSON 파일에서 차트 정보를 읽어오는 함수
        private string LoadLocalChartData() {
            // 로컬 JSON 파일 경로 설정
            string filePath = "Assets/ChartInfo.json";
            
            // 파일 읽기
            if (File.Exists(filePath)) {
                return File.ReadAllText(filePath);
            } else {
                return null;
            }
        }

        // 로컬에서 가져온 차트 정보를 처리하는 함수
        private void ProcessChartInfoLocally(ChartData chartInfo, Action<bool, string, string, string> func) {
            // 처리 로직 작성
            // 예시: chartInfo를 이용한 처리 로직
            // func.Invoke(true, "", "", ""); // 성공 시 콜백 호출
        }

        // 인게임씬으로 이동가는 함수
        private void InGameStart() {
            loadingText.text = "게임 시작하는 중";
            _initializeStep.Clear();
            SceneManager.LoadScene("Main_scene");
        }

        // 추가: JSON 데이터 구조 정의
        [Serializable]
        public class ChartData {
            public string name;
            // 필요한 다른 필드들 추가
        }

        [Serializable]
        public class ChartDataList {
            public List<ChartData> charts;
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
    }
}