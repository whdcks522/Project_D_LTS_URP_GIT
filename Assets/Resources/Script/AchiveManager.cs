using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchiveManager : MonoBehaviour
{
    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject uiNotice;
    enum Achive { UnlockPotate, UnLockBean}
    Achive[] achives;

    private void Awake()
    {
        //주어진 열거형(Enum)의 데이터를 모두 가져오는 함수
        achives = (Achive[])Enum.GetValues(typeof(Achive));//achives 초기화
        if (!PlayerPrefs.HasKey("MyData"))Init();//처음이라면 저장
    }

    private void Init()//저장
    {
        PlayerPrefs.SetInt("MyData", 1);//처음인지 확인용 1 저장
        foreach (Achive achive in achives) //foreach문으로 업적 초기화
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);//저장하기
        }
    }

    private void Start()
    {
        UnlockCharacter();
    }

    void UnlockCharacter() //해금 이미지 변화
    {
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            string achiveName = achives[index].ToString();//achives의 문자열을 활용
            bool isUnLock = PlayerPrefs.GetInt(achiveName) == 1 ? true : false;//위의 문자열을 활용해 playerPref에서 검색
            lockCharacter[index].SetActive(!isUnLock);
            unlockCharacter[index].SetActive(isUnLock);
        }
    }

    //저장하는 측
    private void LateUpdate()
    {
        foreach (Achive achive in achives) 
        {
            //CheckAchive(achive);
        }
    }

    /*
    void CheckAchive(Achive achive) 
    {
        bool isAchive = false;
        switch (achive) 
        {
            case Achive.UnlockPotate:
                isAchive = GameManager.instance.kill >= 10;//10마리 이상 사살
                break;
            case Achive.UnLockBean:
                isAchive = GameManager.instance.gameTime == GameManager.instance.maxGameTime;//타임 아웃
                break;
        }

        if (isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0) //성립 했으면서 처음이면
        {
            PlayerPrefs.SetInt(achive.ToString(), 1);//업적 저장

            for (int index = 0; index <  uiNotice.transform.childCount; index++) 
            {
                bool isActive = index == (int)achive;//enum의 숫자화 이용
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }

            StartCoroutine(NoticeRoutine());
        }
    }
    
  */

}
