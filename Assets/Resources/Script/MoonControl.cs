using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonControl : MonoBehaviour
{
    public float value = 0f;
    public float addValue = 1f;
    private void Update()
    {
        //�� ȸ��
        value += addValue * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", value);
    }
}
