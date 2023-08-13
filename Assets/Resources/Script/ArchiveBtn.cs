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
    
    void Start()//로비 플레이어에서 관리
    {
        //이미지 설정
        archiveBtnImage = GetComponent<Image>();
        archiveBtnImage.sprite = archiveData.archiveIcon;
    }

    public void Enter() //버튼에 마우스 가져다 대면
    {
        if (archiveBtnImage.color == Color.white) 
        {
            //텍스트 수정
            archiveTitle.text = archiveData.archiveType.ToString();
            //효과음
            AuthManager.Instance.audioManager.PlaySfx(archiveData.archiveSfx, true);
        }
        else if (archiveBtnImage.color == Color.black)
        {
            //텍스트 수정
            archiveTitle.text = "???";
            //효과음
            AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
        }
        //설명 문
        archiveDesc.text = archiveData.archiveDesc;
        //크기 변경
        gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
    }
    public void Exit() //버튼에 마우스 떼면
    {
        //타이틀 수정
        archiveTitle.text = ""; 
        //설명 문
        archiveDesc.text = "";
        //크기 변경
        gameObject.transform.localScale = new Vector3(2, 2, 2);
    }
}
