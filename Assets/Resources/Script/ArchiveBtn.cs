using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchiveBtn : MonoBehaviour
{
    public ArchiveData archiveData;
    public Text archiveTitle;
    public Text archiveDesc;
    Image archiveBtnImage;
    
    void Start()
    {
        //�̹��� ����
        archiveBtnImage = GetComponent<Image>();
        archiveBtnImage.sprite = archiveData.archiveIcon;
    }

    public void Enter() 
    {
        if (archiveBtnImage.color == Color.white) 
        {
            //�ؽ�Ʈ ����
            archiveTitle.text = archiveData.archiveType.ToString(); 
        }
        else if (archiveBtnImage.color == Color.black)
        {
            //�ؽ�Ʈ ����
            archiveTitle.text = "???";
        }
        //���� ��
        archiveDesc.text = archiveData.archiveDesc;
        //ũ�� ����
        gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
    }
    public void Exit()
    {
        //Ÿ��Ʋ ����
        archiveTitle.text = ""; 
        //���� ��
        archiveDesc.text = "";
        //ũ�� ����
        gameObject.transform.localScale = new Vector3(2, 2, 2);
    }





}
