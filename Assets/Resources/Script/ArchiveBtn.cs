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
        //이미지 설정
        archiveBtnImage = GetComponent<Image>();
        archiveBtnImage.sprite = archiveData.archiveIcon;
    }

    public void Enter() 
    {
        if (archiveBtnImage.color == Color.white) 
        {
            //텍스트 수정
            archiveTitle.text = archiveData.archiveType.ToString(); 
        }
        else if (archiveBtnImage.color == Color.black)
        {
            //텍스트 수정
            archiveTitle.text = "???";
        }
        //설명 문
        archiveDesc.text = archiveData.archiveDesc;
        //크기 변경
        gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
    }
    public void Exit()
    {
        //타이틀 수정
        archiveTitle.text = ""; 
        //설명 문
        archiveDesc.text = "";
        //크기 변경
        gameObject.transform.localScale = new Vector3(2, 2, 2);
    }





}
