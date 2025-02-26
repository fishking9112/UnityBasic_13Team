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
using System.Collections;

namespace UI {
    public class LoadingSceneManager : MonoBehaviour {

        [SerializeField] private Text loadingText;
        [SerializeField] private Slider loadingSlider;

        private int _maxLoadingCount; // 총 뒤끝 함수를 호출할 갯수
        private int _currentLoadingCount; // 현재 뒤끝 함수를 호출한 갯수
        
        private readonly Queue<Action> _initializeStep = new Queue<Action>();

        private static string nextSceneName = "Main_scene"; // 기본값 설정
        
        void Awake() {
            // Awake에서 초기화
            if (string.IsNullOrEmpty(nextSceneName)) {
                nextSceneName = "Main_scene"; // 기본 씬 이름 설정
            }
        }
        
        void Start() {
            StartCoroutine(LoadSceneProcess());
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
            
            try 
            {
                // 게임 데이터 매니저 생성
                var gameDataObj = new GameObject("GameDataManager");
                var gameDataManager = gameDataObj.AddComponent<GameDataManager>();
                DontDestroyOnLoad(gameDataObj);

                // UserData를 체크하여 신규/기존 유저 구분
                var userData = new UserData();
                bool isNewUser = !userData.LoadFromJson();

                if (isNewUser)
                {
                    Debug.Log("신규 유저 - 새 데이터 생성");
                    gameDataManager.CreateNewUserData();
                    
                    // 모든 데이터 저장 확인
                    Debug.Log("모든 데이터 저장 확인");
                    gameDataManager.SaveAllData();
                    
                    // 저장된 파일 확인 로그 추가
                    CheckSavedFiles();
                    
                    _currentLoadingCount++;
                    loadingSlider.value = _currentLoadingCount;
                }
                else
                {
                    Debug.Log("기존 유저 - 데이터 로드");
                    gameDataManager.LoadExistingUserData();
                    
                    // 여기에 SaveAllData 호출 추가
                    gameDataManager.SaveAllData();
                    
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

        // 저장된 파일 확인 메서드 추가
        private void CheckSavedFiles()
        {
            string persistentPath = Application.persistentDataPath;
            string localLowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Low", "DefaultCompany", "UnityBasic_13Team_Project");
            
            Debug.Log($"Application.persistentDataPath: {persistentPath}");
            Debug.Log($"LocalLow 경로: {localLowPath}");
            
            // persistentDataPath 확인
            if (Directory.Exists(persistentPath))
            {
                string[] files = Directory.GetFiles(persistentPath, "*.json");
                Debug.Log($"persistentDataPath에 {files.Length}개의 JSON 파일이 있습니다:");
                foreach (var file in files)
                {
                    Debug.Log($"- {Path.GetFileName(file)}");
                }
            }
            
            // LocalLow 경로 확인
            if (Directory.Exists(localLowPath))
            {
                string[] files = Directory.GetFiles(localLowPath, "*.json");
                Debug.Log($"LocalLow 경로에 {files.Length}개의 JSON 파일이 있습니다:");
                foreach (var file in files)
                {
                    Debug.Log($"- {Path.GetFileName(file)}");
                }
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

        public static void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName)) {
                nextSceneName = sceneName;
                SceneManager.LoadScene("LoadingScene");
            } else {
                Debug.LogError("LoadScene: 씬 이름이 비어 있습니다!");
            }
        }
        
        private IEnumerator LoadSceneProcess()
        {
            // 디버그 로그 추가
            Debug.Log($"LoadSceneProcess 시작: nextSceneName = {nextSceneName}");
            
            // 데이터 매니저 초기화 (없으면 생성)
            if (GameDataManager.Instance == null)
            {
                Debug.LogWarning("GameDataManager가 없어서 새로 생성합니다.");
            }
            
            // 프로그레스 바 초기화
            loadingSlider.value = 0;
            loadingText.text = "로딩 중... 0%";
            
            // 씬 이름 확인 및 기본값 설정
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("nextSceneName이 비어 있어 기본값 'Main_scene'으로 설정합니다.");
                nextSceneName = "Main_scene";
            }
            
            // 비동기 씬 로드
            Debug.Log($"씬 로드 시작: {nextSceneName}");
            AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
            
            if (op == null)
            {
                Debug.LogError($"씬 로드 실패: {nextSceneName}");
                yield break;
            }
            
            op.allowSceneActivation = false;
            
            float timer = 0f;
            float fakeLoadingTime = 1f; // 최소 로딩 시간
            
            while (!op.isDone)
            {
                timer += Time.deltaTime;
                
                // 진행도 계산 (0.9까지만 진행됨)
                float progress = Mathf.Clamp01(Mathf.Min(timer / fakeLoadingTime, op.progress / 0.9f));
                
                // UI 업데이트
                loadingSlider.value = progress;
                loadingText.text = $"로딩 중... {Mathf.FloorToInt(progress * 100)}%";
                
                // 로딩 완료
                if (progress >= 1f)
                {
                    loadingText.text = "로딩 완료!";
                    op.allowSceneActivation = true;
                }
                
                yield return null;
            }
        }
    }
}