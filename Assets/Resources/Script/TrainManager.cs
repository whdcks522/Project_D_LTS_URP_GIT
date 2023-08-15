using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainManager : MonoBehaviourPunCallbacks
{
    GameManager gameManager;
    public Dropdown dropdown;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }
    private void Start()
    {
        //시작 시, 기본 값 설정
        dropdown.value = 5;
    }

    public string DropDownTranslate() //임시 적 소환
    {
        switch (dropdown.value)
        {
            case 0:
                return "Dummy";
            case 1:
                return "EnemyA";
            case 2:
                return "EnemyB";
            case 3:
                return "BossA";
            case 4:
                return "EnemyC";
            case 5:
                return "BossB";
            default:
                break;
        }
        return "";
    }

    public void DropDownSet()//소환할 적 선택
    {
        gameManager.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
    }

    #region TmpScene에서 나가기
    public void TmpExit()//TmpScene에서 나가기 버튼을 누름
    {
        photonView.RPC("GotoAuthScene", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void GotoAuthScene() //모두에게 나가라고 알림
    {
        Invoke("GotoAuthSceneEnd", 0.25f);
    }

    void GotoAuthSceneEnd() //실제로 authScene로 나감
    {
        AuthManager.Instance.Destroy();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("AuthScene");  
    }
    #endregion
}
