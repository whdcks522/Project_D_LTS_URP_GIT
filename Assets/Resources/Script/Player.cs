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

    // 다른 이름의 메서드로 변경하거나 삭제할 수 있습니다.
    void KeyInput()
    {

    }


    private void LookAt()
    {
        
    }
}