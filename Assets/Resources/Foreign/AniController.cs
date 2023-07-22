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
        // 메인 카메라를 찾습니다.
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) 
        {
            Vector3 clickPosition = Input.mousePosition;
            clickPosition.z = -mainCamera.transform.position.z; // 클릭 좌표의 z값을 카메라와 동일하게 설정합니다.

            // 클릭한 좌표를 카메라 좌표로 변환합니다.
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(clickPosition);
            worldPosition.x *= -1;
            worldPosition.y *= -1;
            worldPosition.y += upValue;
            target.position = worldPosition;
        }
    }
}
