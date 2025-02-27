using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text equipText; // 장착 표시 텍스트
    [SerializeField] private Image backgroundImage;
    
    private int itemId;
    private bool isEquipped;
    
    // 아이템 데이터 설정
    public void SetItemData(int id, string name, bool equipped, int rating)
    {
        itemId = id;
        isEquipped = equipped;
        
        // 아이템 이미지 설정
        string spriteName = id.ToString();
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
            
            if (itemImage.sprite == null)
            {
                Debug.LogWarning("아이템 이미지를 찾을 수 없습니다: " + id);
            }
        }
        else
        {
            itemImage.sprite = itemSprite;
        }
        
        // 아이템 이름 설정
        if (itemNameText != null)
        {
            itemNameText.text = name;
        }
        
        // 장착 상태 텍스트로 표시
        if (equipText != null)
        {
            equipText.text = equipped ? "E" : "";
            equipText.color = Color.yellow;
            equipText.gameObject.SetActive(equipped);
        }
        else
        {
            // equipText가 없으면 생성
            CreateEquipText(equipped);
        }
        
        // 등급에 따른 배경색 설정
        SetRarityColor(rating);
    }
    
    // 장착 텍스트 생성
    private void CreateEquipText(bool equipped)
    {
        // 기존 텍스트가 없으면 새로 생성
        GameObject textObj = new GameObject("EquipText");
        textObj.transform.SetParent(transform, false);
        
        equipText = textObj.AddComponent<Text>();
        equipText.text = equipped ? "E" : "";
        equipText.color = Color.yellow;
        equipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        equipText.fontSize = 24;
        equipText.fontStyle = FontStyle.Bold;
        equipText.alignment = TextAnchor.UpperRight;
        
        // 위치 설정
        RectTransform rectTransform = equipText.rectTransform;
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(-5, -5);
        
        // 활성화 상태 설정
        equipText.gameObject.SetActive(equipped);
    }
    
    private void SetRarityColor(int rating)
    {
        Color rarityColor = Color.white;
        
        switch (rating)
        {
            case 1: // 일반
                rarityColor = new Color(0.8f, 0.8f, 0.8f);
                break;
            case 2: // 고급
                rarityColor = new Color(0.0f, 0.7f, 0.0f);
                break;
            case 3: // 희귀
                rarityColor = new Color(0.0f, 0.4f, 0.8f);
                break;
            case 4: // 전설
                rarityColor = new Color(0.8f, 0.4f, 0.0f);
                break;
        }
        
        backgroundImage.color = rarityColor;
    }
    
    // 아이템 ID 가져오기
    public int GetItemId()
    {
        return itemId;
    }
    
    // 장착 상태 가져오기
    public bool IsEquipped()
    {
        return isEquipped;
    }
    
    // 장착 상태 설정
    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
        
        // 장착 텍스트 업데이트
        if (equipText != null)
        {
            equipText.text = equipped ? "E" : "";
            equipText.gameObject.SetActive(equipped);
        }
    }
} 