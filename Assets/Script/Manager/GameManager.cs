using System.Collections;
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스 추가

// Assets.Script.UI 네임스페이스 제거 (존재하지 않음)

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController player { get; private set; }

    private EnemyManager enemyManager;

    public ObjectPooling objectPooling;

    public static bool isFirstLoading = true;

    public GameObject mapObject;

    public bool doorIsOpen = false;

    [SerializeField] private GameObject portalObject; // Inspector에서 할당 가능
    [SerializeField] private GameObject portalTrigger; // Inspector에서 할당 가능

    private void Awake()
    {
        Instance = this;
        player = FindObjectOfType<PlayerController>();
        player.Init(this);

        enemyManager = GetComponentInChildren<EnemyManager>();

        objectPooling = GetComponentInChildren<ObjectPooling>();

        Init();
    }


    // 게임 매니저가 DontDestroyOnLoad 로 설정되기 때문에 , 씬이 넘어가도 바뀔 값은 Init 에서 따로 초기화 해준다.
    public void Init()
    {
        //맵 정보 다시 가져오기.
        mapObject = GameObject.FindGameObjectWithTag("Map");

        //플레이어 초기 위치 정보 가져오기 ( 맵 마다 다르니까 새로 받아준다 )
        player.transform.position = mapObject.transform.Find("PlayerSpawn").position;

        // 문 단속
        doorIsOpen = false;

        // 포탈 참조 찾기 (Inspector에서 할당하지 않은 경우)
        if (portalObject == null)
        {
            portalObject = GameObject.Find("Potal"); // 또는 "Portal"
            
            if (portalObject != null && portalTrigger == null)
            {
                Transform triggerTransform = portalObject.transform.Find("PotalTrigger");
                if (triggerTransform != null)
                {
                    portalTrigger = triggerTransform.gameObject;
                }
            }
        }
        
        // 포탈 트리거 비활성화
        if (portalTrigger != null)
        {
            portalTrigger.SetActive(false);
        }
    }

    private void Start()
    {
        // UI 매니저 초기화 - 타입 이름으로 직접 찾기
        var uiManager = FindObjectOfType(System.Type.GetType("InGameUIManager"));
        if (uiManager == null)
        {
            // 다른 방법으로 시도
            var uiManagers = FindObjectsOfType<MonoBehaviour>();
            foreach (var manager in uiManagers)
            {
                if (manager.GetType().Name == "InGameUIManager")
                {
                    uiManager = manager;
                    break;
                }
            }
            
            if (uiManager == null)
            {
                Debug.LogWarning("InGameUIManager를 찾을 수 없습니다. UI가 제대로 작동하지 않을 수 있습니다.");
            }
        }
        player.Init(this);
    }

    public void OpenNextDungeon()
    {
        if (!doorIsOpen)
        {
            doorIsOpen = true;
            
            Debug.Log("OpenNextDungeon !");
            
            // 저장된 참조 사용
            if (portalTrigger != null)
            {
                portalTrigger.SetActive(true);
            }
            else
            {
                Debug.LogWarning("포탈 트리거 참조가 없습니다. 씬에 'Potal/PotalTrigger' 오브젝트가 존재하는지 확인하세요.");
            }
        }
    }
}
