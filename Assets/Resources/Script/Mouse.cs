using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoreanTyper;
using System;

public class Mouse : MonoBehaviour
{
    public GameManager gameManager;
    public Image mouseImage;
    public Animator anim;
    Text mouseTextView;
    SkinnedMeshRenderer skinnedMeshRenderer;
    WaitForSeconds wait06 = new WaitForSeconds(0.6f);
    WaitForSeconds wait001 = new WaitForSeconds(0.01f);
    bool isMax;//처음에는 ui애니메이션과 겹치지 않기 위해서 1초 대기
    bool invisibie;
    private void Awake() 
    {
        //초기화
        gameManager = GameManager.Instance;
        mouseTextView = mouseImage.gameObject.transform.GetChild(0).GetComponent<Text>();
        skinnedMeshRenderer = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
        VisibleDissolve();
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !invisibie) 
        {
            gameManager.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
           //글자 애니메이션
            StartCoroutine(TypingRoutine(
                $" 현재는 {gameManager.curStage} 스테이지 입니다.\n 적은 {gameManager.mouseText}입니다.\n 총 수는 {gameManager.MaxEnemiesCount}체 입니다."
                ));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //충돌했을 때 UI 창 활성화
        if (other.gameObject.tag == "Player" && !invisibie)
            isMaxfalse();
    }

    //경계에 들어가서 스테이지 시작
    public void isMaxfalse() 
    {
        if(isMax)
        anim.SetTrigger("isMin");

        isMax = false;
    }

    #region 대화 시작
    IEnumerator TypingRoutine(string str) 
    {
        anim.SetTrigger("isMax");
        isMax = true;
        mouseTextView.text = "";
        yield return wait06;

        int typingLength = str.GetTypingLength();

        for (int index = 0; index <= typingLength; index++) 
        {
            mouseTextView.text = str.Typing(index);
            yield return wait001;
        }
    }
    #endregion

    #region 왜곡장
    public void InvisibleDissolve() // 점차 안보이게 되는 것
    {
        StopCoroutine(Dissolve(false));
        StartCoroutine(Dissolve(true));
    }
    public void VisibleDissolve() //점차 보이게 되는 것 
    {
        
        StopCoroutine(Dissolve(true));
        StartCoroutine(Dissolve(false));
    }
    private IEnumerator Dissolve(bool b)
    {
        if (b) invisibie = true;
        float firstValue = b ? 0f : 1f;      //true는 InvisibleDissolve(2초)
        float targetValue = b ? 1f : 0f;     //false는 VisibleDissolve(3초)

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;//진행률
            float value = Mathf.Lerp(firstValue, targetValue, progress);

            elapsedTime += Time.deltaTime;

            skinnedMeshRenderer.material.SetFloat("_AlphaControl", value);
            yield return null;
        }
        skinnedMeshRenderer.material.SetFloat("_AlphaControl", targetValue);
        if (!b) invisibie = false;
    }
    #endregion
}
