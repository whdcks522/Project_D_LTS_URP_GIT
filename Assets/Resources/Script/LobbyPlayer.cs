using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class LobbyPlayer : MonoBehaviour
{
    public Transform target;

    public Camera mainCamera;
    public float xValue;
    public float yValue;
    public float zValue;

    SkinnedMeshRenderer[] skinnedMeshRenderer = new SkinnedMeshRenderer[2];
    AudioManager audioManager;
    AuthManager authManager;

    //���� ����
    public GameObject archiveGameObject;

    private void Awake()
    {
        //�ʱ�ȭ
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
        //1�� ����
        skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
        //2�� ��ü
        skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));

        authManager = AuthManager.Instance;
    }
    
    void OnEnable()
    {
        //�ְ���
        StartCoroutine(Dissolve());
        //��� ���� �ʱ�ȭ
        audioManager = authManager.GetComponent<AudioManager>();//�̴�� ����
        audioManager.PlayBgm(AudioManager.Bgm.Lobby);

        //���� ����
        Image[] archiveImages = archiveGameObject.GetComponentsInChildren<Image>();
        int arrSize = System.Enum.GetValues(typeof(AuthManager.ArchiveType)).Length;
        for (int index = 0; index < arrSize; index++)
        {
            //Debug.Log(index + ":"+authManager.originAchievements.Arr[index]);
            if (authManager.originAchievements.Arr[index] == 1) 
                archiveImages[index].color = Color.white;
        }
    }

    public void DropDownSet()
    {
        audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
    }

    void FixedUpdate()
    {
        #region �þ� ����
        Vector3 clickPosition = Input.mousePosition;
            clickPosition.z = -mainCamera.transform.position.z; // Ŭ�� ��ǥ�� z���� ī�޶�� �����ϰ� �����մϴ�.

            // Ŭ���� ��ǥ�� ī�޶� ��ǥ�� ��ȯ�մϴ�.
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(clickPosition);

            //��ġ ����
            worldPosition.x += xValue;
            worldPosition.y += yValue;
            worldPosition.z = zValue;
            target.position = worldPosition;
        #endregion
    }

    #region �ְ���
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
    #endregion
}
