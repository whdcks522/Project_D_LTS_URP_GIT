using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    //[Header("Lobby")]
    Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        KeyInput();
    }

    // �ٸ� �̸��� �޼���� �����ϰų� ������ �� �ֽ��ϴ�.
    void KeyInput()
    {

    }


    private void LookAt()
    {
        
    }
}