using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultPanel : MonoBehaviour
{
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GridLayoutGroup gridLayout;
    
    [Header("등급 색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.magenta;
    [SerializeField] private Color legendaryColor = Color.yellow;
    
    private System.Action onConfirmClicked;
    
    private void Awake()
    {
        Debug.Log($"GachaResultPanel Awake: {gameObject.name}");
        
        // 자식 오브젝트 로깅
        Debug.Log("자식 오브젝트 목록:");
        foreach (Transform child in transform)
        {
            Debug.Log($"- {child.name} (활성화: {child.gameObject.activeSelf})");
        }
        
        // 버튼 이벤트 연결
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            Debug.Log($"확인 버튼 이벤트 연결 완료: {confirmButton.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("확인 버튼이 할당되지 않았습니다.");
        }
    }
    
    public void Initialize(List<GachaManager.WeaponData> items, System.Action onConfirm)
    {
        onConfirmClicked = onConfirm;
        
        // 전체 패널 활성화
        gameObject.SetActive(true);
        
        // 패널 위치 및 크기 설정
        RectTransform panelRect = GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // 전체 화면을 차지하도록 설정
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // 최상위로 가져오기
            transform.SetAsLastSibling();
        }
        
        // 버튼 활성화 (중요!)
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(true);
            
            // 버튼 위치 조정
            RectTransform buttonRect = confirmButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // 버튼이 항상 보이도록 위치 조정
                buttonRect.anchorMin = new Vector2(0.5f, 0);
                buttonRect.anchorMax = new Vector2(0.5f, 0);
                buttonRect.pivot = new Vector2(0.5f, 0);
                buttonRect.anchoredPosition = new Vector2(0, 100); // 패널 하단에서 100픽셀 위
                buttonRect.sizeDelta = new Vector2(200, 60);
            }
            
            Debug.Log($"확인 버튼 활성화: {confirmButton.gameObject.name}");
        }
        
        // 기존 아이템 제거
        if (itemContainer != null)
        {
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("아이템 컨테이너가 할당되지 않았습니다.");
            return;
        }
        
        // 그리드 레이아웃 설정
        if (gridLayout != null)
        {
            if (items.Count > 1)
            {
                gridLayout.enabled = true;
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 5; // 5열
            }
            else
            {
                gridLayout.enabled = false;
            }
        }
        
        // 아이템 생성
        if (itemPrefab != null)
        {
            foreach (var item in items)
            {
                GameObject itemObj = Instantiate(itemPrefab, itemContainer);
                SetupItemUI(itemObj, item);
            }
        }
    }
    
    private void SetupItemUI(GameObject itemObj, GachaManager.WeaponData item)
    {
        // 이미지 컴포넌트 찾기
        Image itemImage = itemObj.GetComponent<Image>();
        if (itemImage == null) itemImage = itemObj.GetComponentInChildren<Image>();
        
        // 텍스트 컴포넌트 찾기
        Text itemNameText = itemObj.GetComponentInChildren<Text>();
        
        // 이미지 설정
        if (itemImage != null)
        {
            // 스프라이트 로드
            Sprite itemSprite = Resources.Load<Sprite>($"item/{item.id}");
            
            if (itemSprite == null)
            {
                // 타입별 기본 스프라이트 사용
                int type = item.id / 1000;
                itemSprite = Resources.Load<Sprite>($"item/{type}001");
            }
            
            if (itemSprite != null)
            {
                itemImage.sprite = itemSprite;
            }
            
            // 등급에 따라 색상 설정
            Color itemColor = normalColor;
            switch (item.rating)
            {
                case 2: itemColor = rareColor; break;
                case 3: itemColor = epicColor; break;
                case 4: itemColor = legendaryColor; break;
            }
            
            itemImage.color = itemColor;
        }
        
        // 이름 텍스트 설정
        if (itemNameText != null)
        {
            itemNameText.text = item.name;
        }
    }
    
    private void OnConfirmButtonClicked()
    {
        Debug.Log("확인 버튼 클릭됨");
        
        // 인벤토리 UI 새로고침 추가 (이중 안전장치)
        var inventoryManager = FindObjectOfType<EquipmentInventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RefreshInventory();
            Debug.Log("GachaResultPanel에서 인벤토리 UI 새로고침 완료");
        }
        
        onConfirmClicked?.Invoke();
        gameObject.SetActive(false);
    }
} 