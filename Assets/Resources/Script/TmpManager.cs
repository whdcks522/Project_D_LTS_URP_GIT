using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TmpManager : MonoBehaviourPunCallbacks//일반적인 MonoBehaviour와 달리 포톤.pun의 이벤트를 감지
{
    private readonly string gameVersion = "1";//같은 게임이라도 버전이 다르면 매칭이 안돼야 함
    //마스터 서버에 접속한 순간 무언가를 실행
    public Text stateText;
    public GameObject fireImage;
    public Button joinButton;
    int State = 1;
    AudioManager audioManager;
    private void Awake()
    {
        audioManager = AuthManager.Instance.GetComponent<AudioManager>();
    }

    public override void OnConnectedToMaster()//연결 설정하면 //자동으로 실행됨
    {
        if (State == 1) 
        {
            stateText.text = "2온라인: 마스터 서버에 접속 됨";
            //플레이어 닉네임
            PhotonNetwork.LocalPlayer.NickName = "NickName" + Random.Range(0, 10000);//NickNameInput.text
            State = 2;                                                          //자동 실행을 위한 상태 변환

            OnConnect();
        }
        
    }

    public override void OnDisconnected(DisconnectCause cause)//연결 실패했거나, 이미 접속 중인데 끊김 //자동 실행됨
    {
        if (State == 2) 
        {
            //joinButton.interactable = false;
            stateText.text = $"3오프라인: 연결 실패함: {cause.ToString()}";
            //재시도
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnConnect()//조인 버튼 실행했을 때
    {
        fireImage.SetActive(true);

        if (State == 1)
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);

            //로비에 진입함과 동시에 마스터 서버(=포톤 클라우드 서버, 매치매이킹을 위함)에 진입 시도
            PhotonNetwork.GameVersion = gameVersion;//게임 버전
            PhotonNetwork.ConnectUsingSettings();//설정 정보(ex) 게임 버전 등(이번에는 게임 버전만 가능))를 갖고 마스터 서버에 접속 시도----------->
            
            joinButton.interactable = false;
            stateText.text = "1마스터 서버에 연결 중..";
        }
        else if (State == 2) 
        {
            //joinButton.interactable = false;
            if (PhotonNetwork.IsConnected) //누르는 순간 끊길 수도 있으모로, 안전 장치임
            {
                stateText.text = "4 랜덤한 방에 접속 시도";
                PhotonNetwork.JoinRandomRoom();//랜덤한 룸에 접속 시도(빈 방이 없다면 당연히 실패)------------>
            }
            else //접속 불가할 경우
            {
                stateText.text = "3오프라인: 연결 실패함: 재시도 해봐라}";
                //재시도
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)//대부분 랜덤 방에 들어가려는데 방이 없어서 실패
    {
        if (State == 2) 
        {

            //새로 방을 만들고, 자신이 방장이 됨
            stateText.text = "5 빈 방이 없으므로, 직접 만듬";
            //변수(방 이름, 조건(최대 2명))
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });//네트워크 상에서 새로운 방 제작 후, 들어감----------->
        }
    }

    public override void OnJoinedRoom()//룸에 참가 완료시 자동 실행(방 접속에 성공하거나, 직접 방을 만드는 경우)
    {
        audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);

        stateText.text = "6 방에 들어옴";
        joinButton.interactable = true;
        //씬 매니저로 이동하면 나만 넘어가고, 다른 사람은 같이 안넘어감(각각 써서, 동기화가 안됨)
        PhotonNetwork.LoadLevel("TmpScene");//방장이 하면 나머지도 자동으로 끌려옴, 동기화도 자동으로 됨
    }

    //책으로 이동
    public void LoadBook() 
    {
        SceneManager.LoadScene("BookScene");
    }
}
