using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineMgr : MonoBehaviour
{
    public GameObject[] SlotSkillObject;
    public Button[] Slot;

    public Sprite[] SkillSprite;
    
    // 각 슬롯 아래에 표시할 설명 텍스트
    public Text[] SlotDescriptionTexts;

    [System.Serializable]
    public class DisplayItemSlot
    {
        public List<Image> SlotSprite = new List<Image>();
    }
    public DisplayItemSlot[] DisplayItemSlots;

    public Image DisplayResultImage;
    public Text DisplayResultText;

    public List<int> StartList = new List<int>();
    public List<int> ResultIndexList = new List<int>();
    int ItemCnt = 3;
    int[] answer = { 2, 3, 1 };
    
    // 슬롯 머신 초기화 완료 여부
    private bool isInitialized = false;
    
    // 슬롯 머신 초기화 메서드 (외부에서 호출 가능)
    public void InitializeSlotMachine()
    {
        // 이미 초기화되어 있으면 리셋
        if (isInitialized)
        {
            ResetSlotMachine();
        }
        
        StartList.Clear();
        ResultIndexList.Clear();
        
        for (int i = 0; i < ItemCnt * Slot.Length; i++)
        {
            StartList.Add(i % SkillSprite.Length);
        }

        for (int i = 0; i < Slot.Length; i++)
        {
            for (int j = 0; j < ItemCnt; j++)
            {
                Slot[i].interactable = false;

                int randomIndex = Random.Range(0, StartList.Count);
                if (i == 0 && j == 1 || i == 1 && j == 0 || i == 2 && j == 2)
                {
                    ResultIndexList.Add(StartList[randomIndex]);
                    
                    // 결과 슬롯의 설명 텍스트 업데이트
                    UpdateSlotDescription(i, StartList[randomIndex]);
                }
                DisplayItemSlots[i].SlotSprite[j].sprite = SkillSprite[StartList[randomIndex]];

                if (j == 0)
                {
                    DisplayItemSlots[i].SlotSprite[ItemCnt].sprite = SkillSprite[StartList[randomIndex]];
                }
                StartList.RemoveAt(randomIndex);
            }
        }

        for (int i = 0; i < Slot.Length; i++)
        {
            StartCoroutine(StartSlot(i));
        }
        
        // 결과 이미지 초기화
        if (DisplayResultImage != null)
        {
            DisplayResultImage.gameObject.SetActive(false);
        }
        
        // 결과 텍스트 초기화
        if (DisplayResultText != null)
        {
            DisplayResultText.text = "";
        }
        
        isInitialized = true;
    }
    
    // 슬롯 설명 텍스트 업데이트
    private void UpdateSlotDescription(int slotIndex, int skillIndex)
    {
        if (SlotDescriptionTexts != null && slotIndex < SlotDescriptionTexts.Length && SlotDescriptionTexts[slotIndex] != null)
        {
            int normalizedSkillIndex = skillIndex % SkillSprite.Length;
            SlotDescriptionTexts[slotIndex].text = GetSkillShortDescription(normalizedSkillIndex);
        }
    }
    
    // 슬롯 머신 리셋
    private void ResetSlotMachine()
    {
        // 진행 중인 모든 코루틴 중지
        StopAllCoroutines();
        
        // 슬롯 오브젝트 위치 초기화
        for (int i = 0; i < SlotSkillObject.Length; i++)
        {
            SlotSkillObject[i].transform.localPosition = Vector3.zero;
        }
        
        // 버튼 상호작용 비활성화
        for (int i = 0; i < Slot.Length; i++)
        {
            Slot[i].interactable = false;
        }
        
        // 설명 텍스트 초기화
        if (SlotDescriptionTexts != null)
        {
            foreach (var text in SlotDescriptionTexts)
            {
                if (text != null)
                {
                    text.text = "???";
                }
            }
        }
    }

    // Start 메서드 - 자동 초기화 대신 외부 호출로 변경
    void Start()
    {
        // 초기화는 InGameUIManager에서 호출
    }
    
    IEnumerator StartSlot(int SlotIndex)
    {
        // 슬롯이 돌아가는 동안 설명 텍스트 숨기기
        if (SlotDescriptionTexts != null && SlotIndex < SlotDescriptionTexts.Length && SlotDescriptionTexts[SlotIndex] != null)
        {
            SlotDescriptionTexts[SlotIndex].text = "???";
        }
        
        for (int i = 0; i < (ItemCnt * (6 + SlotIndex * 4) + answer[SlotIndex]) * 2; i++)
        {
            SlotSkillObject[SlotIndex].transform.localPosition -= new Vector3(0, 50f, 0);
            if (SlotSkillObject[SlotIndex].transform.localPosition.y < 50f)
            {
                SlotSkillObject[SlotIndex].transform.localPosition += new Vector3(0, 300f, 0);
            }
            yield return new WaitForSecondsRealtime(0.02f); // Time.timeScale이 0일 때도 작동하도록 수정
        }
        
        // 슬롯이 멈추면 설명 텍스트 표시
        UpdateSlotDescription(SlotIndex, ResultIndexList[SlotIndex]);
        
        for (int i = 0; i < Slot.Length; i++)
        {
            Slot[i].interactable = true;
        }
    }

    public void ClickBtn(int index)
    {
        // 결과 이미지 표시
        if (DisplayResultImage != null)
        {
            DisplayResultImage.gameObject.SetActive(true);
            DisplayResultImage.sprite = SkillSprite[ResultIndexList[index]];
        }
        
        // 결과 텍스트 표시
        if (DisplayResultText != null)
        {
            int skillIndex = ResultIndexList[index] % SkillSprite.Length;
            string skillName = GetSkillName(skillIndex);
            string skillDescription = GetSkillDescription(skillIndex);
            DisplayResultText.text = $"{skillName}\n{skillDescription}";
        }
        
        // 선택한 스킬 적용 (InGameUIManager에서 처리)
        InGameUIManager uiManager = InGameUIManager.Instance;
        if (uiManager != null)
        {
            uiManager.ApplySelectedSkill(ResultIndexList[index] % SkillSprite.Length);
        }
    }
    
    // 스킬 이름 반환
    private string GetSkillName(int index)
    {
        string[] skillNames = { "공격력 증가", "방어력 증가", "최대 체력 증가", "이동 속도 증가", "크리티컬 확률 증가" };
        if (index >= 0 && index < skillNames.Length)
        {
            return skillNames[index];
        }
        return "알 수 없는 스킬";
    }
    
    // 스킬 설명 반환
    private string GetSkillDescription(int index)
    {
        string[] skillDescriptions = { 
            "공격력이 5 증가합니다.", 
            "방어력이 3 증가합니다.", 
            "최대 체력이 20 증가합니다.", 
            "이동 속도가 0.2 증가합니다.", 
            "크리티컬 확률이 5% 증가합니다." 
        };
        if (index >= 0 && index < skillDescriptions.Length)
        {
            return skillDescriptions[index];
        }
        return "효과 없음";
    }
    
    // 짧은 스킬 설명 반환 (슬롯 아래 표시용)
    private string GetSkillShortDescription(int index)
    {
        string[] shortDescriptions = {
            "공격력 +5",
            "방어력 +3",
            "체력 +20",
            "속도 +0.2",
            "크리티컬 +5%"
        };
        if (index >= 0 && index < shortDescriptions.Length)
        {
            return shortDescriptions[index];
        }
        return "???";
    }
}