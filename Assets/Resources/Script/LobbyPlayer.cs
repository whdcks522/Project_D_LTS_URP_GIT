using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{
    public Transform target;

    public Camera mainCamera;
    public float xValue;
    public float yValue;
    public float zValue;

    SkinnedMeshRenderer[] skinnedMeshRenderer = new SkinnedMeshRenderer[2];

    private void Awake()
    {
        //�ʱ�ȭ
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
        //1�� ����
        skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
        //2�� ��ü
        skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));
    }

    void OnEnable()
    {
        StartCoroutine(Dissolve());
    }


    void FixedUpdate()
    {

            Vector3 clickPosition = Input.mousePosition;
            clickPosition.z = -mainCamera.transform.position.z; // Ŭ�� ��ǥ�� z���� ī�޶�� �����ϰ� �����մϴ�.

            // Ŭ���� ��ǥ�� ī�޶� ��ǥ�� ��ȯ�մϴ�.
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(clickPosition);

            //��ġ ����
            worldPosition.x += xValue;
            worldPosition.y += yValue;
            worldPosition.z = zValue;
            target.position = worldPosition;
        
    }

    public IEnumerator Dissolve()
    {
        float firstValue = 1f;      //true�� InvisibleDissolve(2��)
        float targetValue = 0f;     //false�� VisibleDissolve(3��)

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;//�����
            float value = Mathf.Lerp(firstValue, targetValue, progress);

            elapsedTime += Time.deltaTime;

            skinnedMeshRenderer[0].material.SetFloat("_AlphaControl", value);
            skinnedMeshRenderer[1].material.SetFloat("_AlphaControl", value);
            yield return null;
        }
        skinnedMeshRenderer[0].material.SetFloat("_AlphaControl", targetValue);
        skinnedMeshRenderer[1].material.SetFloat("_AlphaControl", targetValue);
    }
}
