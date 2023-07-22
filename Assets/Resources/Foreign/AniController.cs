using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AniController : MonoBehaviour
{
    public Transform target;

    private Camera mainCamera;
    public int upValue;

    private void Start()
    {
        // ���� ī�޶� ã���ϴ�.
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) 
        {
            Vector3 clickPosition = Input.mousePosition;
            clickPosition.z = -mainCamera.transform.position.z; // Ŭ�� ��ǥ�� z���� ī�޶�� �����ϰ� �����մϴ�.

            // Ŭ���� ��ǥ�� ī�޶� ��ǥ�� ��ȯ�մϴ�.
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(clickPosition);
            worldPosition.x *= -1;
            worldPosition.y *= -1;
            worldPosition.y += upValue;
            target.position = worldPosition;
        }
    }
}
