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

    //업적 관리
    public GameObject archiveGameObject;

    private void Awake()
    {
        //초기화
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
        //1번 관절
        skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
        //2번 몸체
        skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));

        authManager = AuthManager.Instance;
    }
    
    void OnEnable()
    {
        //왜곡장
        StartCoroutine(Dissolve());
        //배경 음악 초기화
        audioManager = authManager.GetComponent<AudioManager>();//이대로 두자
        audioManager.PlayBgm(AudioManager.Bgm.Lobby);

        //업적 관리
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
        #region 시야 조절
        Vector3 clickPosition = Input.mousePosition;
            clickPosition.z = -mainCamera.transform.position.z; // 클릭 좌표의 z값을 카메라와 동일하게 설정합니다.

            // 클릭한 좌표를 카메라 좌표로 변환합니다.
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(clickPosition);

            //위치 조정
            worldPosition.x += xValue;
            worldPosition.y += yValue;
            worldPosition.z = zValue;
            target.position = worldPosition;
        #endregion
    }

    #region 왜곡장
    public IEnumerator Dissolve()
    {
        float firstValue = 1f;      //true는 InvisibleDissolve(2초)
        float targetValue = 0f;     //false는 VisibleDissolve(3초)

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;//진행률
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
