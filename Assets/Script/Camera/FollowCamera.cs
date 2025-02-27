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

        //위치
        Vector3 pos = target.position;
        pos.y = cameraHeight;

        transform.position = pos;
        //회전값 초기화
        transform.rotation = Quaternion.identity;
        //아래로 90도 내려보게 한다
        float rotX = 90.0f;
        transform.rotation = Quaternion.Euler(rotX, 0f, 0f);
    }

    private void LateUpdate()
    {

        Vector3 pos = target.position;
        pos.y = cameraHeight;
        //x축 고정
        //pos.x = 0.0f;

        transform.position = pos;
    }
}
