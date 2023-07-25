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
        //�־��� ������(Enum)�� �����͸� ��� �������� �Լ�
        achives = (Achive[])Enum.GetValues(typeof(Achive));//achives �ʱ�ȭ
        if (!PlayerPrefs.HasKey("MyData"))Init();//ó���̶�� ����
    }

    private void Init()//����
    {
        PlayerPrefs.SetInt("MyData", 1);//ó������ Ȯ�ο� 1 ����
        foreach (Achive achive in achives) //foreach������ ���� �ʱ�ȭ
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);//�����ϱ�
        }
    }

    private void Start()
    {
        UnlockCharacter();
    }

    void UnlockCharacter() //�ر� �̹��� ��ȭ
    {
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            string achiveName = achives[index].ToString();//achives�� ���ڿ��� Ȱ��
            bool isUnLock = PlayerPrefs.GetInt(achiveName) == 1 ? true : false;//���� ���ڿ��� Ȱ���� playerPref���� �˻�
            lockCharacter[index].SetActive(!isUnLock);
            unlockCharacter[index].SetActive(isUnLock);
        }
    }

    //�����ϴ� ��
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
                isAchive = GameManager.instance.kill >= 10;//10���� �̻� ���
                break;
            case Achive.UnLockBean:
                isAchive = GameManager.instance.gameTime == GameManager.instance.maxGameTime;//Ÿ�� �ƿ�
                break;
        }

        if (isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0) //���� �����鼭 ó���̸�
        {
            PlayerPrefs.SetInt(achive.ToString(), 1);//���� ����

            for (int index = 0; index <  uiNotice.transform.childCount; index++) 
            {
                bool isActive = index == (int)achive;//enum�� ����ȭ �̿�
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }

            StartCoroutine(NoticeRoutine());
        }
    }
    
  */

}
