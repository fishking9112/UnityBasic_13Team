using UnityEngine;

public class PlayerHealthBarController : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // 머리 위 오프셋
    
    private GameObject healthBarInstance;
    
    private void Start()
    {
        // HP바 생성 및 위치 설정
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
        
        // HP바를 플레이어의 자식으로 설정
        healthBarInstance.transform.SetParent(transform);
    }
    
    private void LateUpdate()
    {
        // HP바 위치 업데이트 (플레이어가 움직일 때)
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + offset;
        }
    }
}