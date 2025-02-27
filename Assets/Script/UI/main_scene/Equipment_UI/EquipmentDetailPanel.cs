using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class EquipmentDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text itemTypeText;
    [SerializeField] private Text itemRatingText;
    [SerializeField] private Text itemCapabilityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text equipButtonText;

    private int currentItemId;
    private int currentItemIndex;
    private bool isEquipped;
    private int itemType;
    private EquipmentInventoryManager inventoryManager;

    private void Start()
    {
        // 버튼 이벤트 등록
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        // 초기에는 패널 비활성화
        gameObject.SetActive(false);
        
        // 인벤토리 매니저 참조 가져오기
        inventoryManager = FindObjectOfType<EquipmentInventoryManager>();
    }

    // 패널 초기화 및 표시
    public void ShowPanel(int itemId, string itemName, int type, int rating, int capability, bool equipped, int itemIndex)
    {
        // 현재 아이템 정보 저장
        currentItemId = itemId;
        currentItemIndex = itemIndex;
        isEquipped = equipped;
        itemType = type;

        // UI 업데이트
        itemNameText.text = itemName;
        itemTypeText.text = GetItemTypeText(type);
        itemRatingText.text = GetRatingText(rating);
        itemCapabilityText.text = capability.ToString();
        
        // 아이템 이미지 설정
        string spriteName = itemId.ToString();
        Sprite itemSprite = Resources.Load<Sprite>("item/" + spriteName);
        
        if (itemSprite == null)
        {
            // 스프라이트를 직접 이름으로 찾기
            Sprite[] allSprites = Resources.LoadAll<Sprite>("item");
            foreach (Sprite sprite in allSprites)
            {
                if (sprite.name == spriteName)
                {
                    itemImage.sprite = sprite;
                    break;
                }
            }
        }
        else
        {
            itemImage.sprite = itemSprite;
        }
        
        // 장착 버튼 텍스트 업데이트
        UpdateEquipButtonText();
        
        // 패널 활성화
        gameObject.SetActive(true);
    }

    // 장착 버튼 텍스트 업데이트
    private void UpdateEquipButtonText()
    {
        equipButtonText.text = isEquipped ? "장착 해제" : "장착";
    }

    // 장착/해제 버튼 클릭 처리
    private void OnEquipButtonClicked()
    {
        if (inventoryManager != null)
        {
            Debug.Log($"장착 버튼 클릭: 아이템 ID {currentItemId}, 인덱스 {currentItemIndex}, 현재 상태 {(isEquipped ? "장착" : "미장착")}");
            
            // 장착 상태 토글 - 인덱스 기반으로 호출
            bool success = inventoryManager.ToggleEquipItem(currentItemIndex, itemType);
            
            if (success)
            {
                // 상태 업데이트
                isEquipped = !isEquipped;
                UpdateEquipButtonText();
                
                Debug.Log($"장착 상태 변경 성공: 아이템 ID {currentItemId}, 인덱스 {currentItemIndex}, 새 상태 {(isEquipped ? "장착" : "미장착")}");
                
                // 인벤토리 UI 강제 새로고침
                inventoryManager.RefreshInventory();
            }
            else
            {
                Debug.LogError($"장착 상태 변경 실패: 아이템 ID {currentItemId}, 인덱스 {currentItemIndex}");
            }
        }
        else
        {
            Debug.LogError("인벤토리 매니저를 찾을 수 없습니다.");
        }
    }

    // 닫기 버튼 클릭 처리
    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private string GetItemTypeText(int type)
    {
        string typeString = "알 수 없음";
        switch (type)
        {
            case 1:
                typeString = "무기";
                break;
            case 2:
                typeString = "방어구";
                break;
            case 3:
                typeString = "장신구";
                break;
        }
        return "타입: " + typeString;
    }

    private string GetRatingText(int rating)
    {
        string ratingString = "일반";
        switch (rating)
        {
            case 1:
                ratingString = "일반";
                break;
            case 2:
                ratingString = "고급";
                break;
            case 3:
                ratingString = "희귀";
                break;
            case 4:
                ratingString = "전설";
                break;
        }
        return "등급: " + ratingString;
    }
} 