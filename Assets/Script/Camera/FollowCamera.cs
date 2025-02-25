using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    [Range(10.0f, 40.0f)]
    [SerializeField]
    private float cameraHeight;

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        target = player.transform;
    }

    void Start()
    {
        if (target == null)
            return;

        //��ġ
        Vector3 pos = target.position;
        pos.y = cameraHeight;

        transform.position = pos;
        //ȸ���� �ʱ�ȭ
        transform.rotation = Quaternion.identity;
        //�Ʒ��� 90�� �������� �Ѵ�
        float rotX = 90.0f;
        transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
    }

    private void LateUpdate()
    {

        Vector3 pos = target.position;
        pos.y = cameraHeight;
        //x�� ����
        pos.x = 0.0f;

        transform.position = pos;
    }
}
