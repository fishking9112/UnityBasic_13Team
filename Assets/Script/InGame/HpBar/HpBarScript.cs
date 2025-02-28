using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBarScript : MonoBehaviour
{
    private GameObject HpBar;


    private void Start()
    {
        HpBar = GameObject.Find("UI/Canvas/HpBar/HpBarSlider");
    }

    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        HpBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0f, -1f));
    }
}
