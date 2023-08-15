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
    bool isMax;//ó������ ui�ִϸ��̼ǰ� ��ġ�� �ʱ� ���ؼ� 1�� ���
    bool invisibie;
    private void Awake() 
    {
        //�ʱ�ȭ
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
           //���� �ִϸ��̼�
            StartCoroutine(TypingRoutine(
                $" ����� {gameManager.curStage} �������� �Դϴ�.\n ���� {gameManager.mouseText}�Դϴ�.\n �� ���� {gameManager.MaxEnemiesCount}ü �Դϴ�."
                ));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //�浹���� �� UI â Ȱ��ȭ
        if (other.gameObject.tag == "Player" && !invisibie)
            isMaxfalse();
    }

    //��迡 ���� �������� ����
    public void isMaxfalse() 
    {
        if(isMax)
        anim.SetTrigger("isMin");

        isMax = false;
    }

    #region ��ȭ ����
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

    #region �ְ���
    public void InvisibleDissolve() // ���� �Ⱥ��̰� �Ǵ� ��
    {
        StopCoroutine(Dissolve(false));
        StartCoroutine(Dissolve(true));
    }
    public void VisibleDissolve() //���� ���̰� �Ǵ� �� 
    {
        
        StopCoroutine(Dissolve(true));
        StartCoroutine(Dissolve(false));
    }
    private IEnumerator Dissolve(bool b)
    {
        if (b) invisibie = true;
        float firstValue = b ? 0f : 1f;      //true�� InvisibleDissolve(2��)
        float targetValue = b ? 1f : 0f;     //false�� VisibleDissolve(3��)

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;//�����
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
