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
    public Animator archiveAnim;
    //아카이브 총 책임자
    public GameObject archiveGameObject;

    private void Awake()
    {
        //초기화
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
        //색 변화
        skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
        skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));

        authManager = AuthManager.Instance;
    }
    
    void OnEnable()
    {
        //왜곡장
        StartCoroutine(Dissolve());
        //배경 음악
        audioManager = authManager.GetComponent<AudioManager>();//�̴�� ����
        audioManager.PlayBgm(AudioManager.Bgm.Lobby);

        //업적 이미지 관리
        Image[] archiveImages = archiveGameObject.GetComponentsInChildren<Image>();
        int arrSize = System.Enum.GetValues(typeof(AuthManager.ArchiveType)).Length;
        for (int index = 0; index < arrSize; index++)
        {
            if (authManager.originAchievements.Arr[index] == 1) 
                archiveImages[index].color = Color.white;
        }
    }

    public void ArchiveAnimControl() 
    {
        if (archiveAnim.GetInteger("Dir") == -1)//왼쪽->오른쪽
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
            archiveAnim.SetInteger("Dir", 1);
        }
        else//왼쪽<-오른쪽
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
            archiveAnim.SetInteger("Dir", -1);
        }         
    }

    public void DropDownSet()//챕터 선택
    {
        audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
    }

    void FixedUpdate()
    {
        #region 시선 관리
        Vector3 clickPosition = Input.mousePosition;
            clickPosition.z = -mainCamera.transform.position.z; // Ŭ�� ��ǥ�� z���� ī�޶�� �����ϰ� �����մϴ�.

            //클릭 지점 저장
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(clickPosition);

            //클릭 지점 보정
            worldPosition.x += xValue;
            worldPosition.y += yValue;
            worldPosition.z = zValue;
            target.position = worldPosition;
        #endregion
    }

    #region 왜곡장
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
