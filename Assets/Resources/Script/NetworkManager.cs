using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Lobby")]
    public InputField RoomInput;
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;
    public Text StatusText;
    public LobbyPlayer lobbyPlayer;

    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;


    #region 방리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {


        if (num == -2)
        {
            AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
            --currentPage;
        }
        else if (num == -1)
        {
            AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
            ++currentPage;
        }
        else 
        {
            AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
            PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        }
        
        MyListRenewal();
    }


    void MyListRenewal()
    {
        
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;//각 페이지의 첫 번째 방의 인덱스
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//서버에서 자동으로 룸의 목록을 가져오는 그거인가 봄
    {
        //아래에 로비에 접근할 때마다 리스트 초기화해서 괜찮음
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)//현재 존재하는 방이라면
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]); //근데 포함하고 있지 않다면, 더한다
                else myList[myList.IndexOf(roomList[i])] = roomList[i];     //받은 것의 인덱스를 추출해 동기화(인원만 바뀐경우 동기화)  
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();

    }
    #endregion

    #region 서버연결
    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
            //네트워크 전체에서 룸에 있는 인원 빼면 로비에 있는 인원 수 나옴
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속";
    }

    private void Start()
    {
        Connect();
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);

    }


    public void Connect() => PhotonNetwork.ConnectUsingSettings();//AuthManager에서 이미 써서 필요 없음

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();//AuthManager에서 이미 써서 필요 없음

    public override void OnJoinedLobby()//로비에 들어오면 닉네임 설정
    {
        if (AuthManager.Instance.User != null)
             PhotonNetwork.LocalPlayer.NickName = AuthManager.Instance.playerEmail;
        else 
             PhotonNetwork.LocalPlayer.NickName = "NickName" + Random.Range(0, 10000);//NickNameInput.text
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        myList.Clear();
    }

    public void Disconnect() 
    {
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
        PhotonNetwork.Disconnect();
    } 
    public override void OnDisconnected(DisconnectCause cause)//*********************8
    {
        SceneManager.LoadScene("AuthScene");
    }
    #endregion

    #region 방
    //**********************************룸 이름이 비어있다면
    public void CreateRoom() //방 생성
    {
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
        PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 2 });//수정함
    }
    public void JoinRandomRoom()//랜덤 방 입장
    {
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
        PhotonNetwork.JoinRandomRoom();
    } 
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }//같은 이름의 룸을 만들면 실패
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }//같은 이름의 룸을 만들면 실패

    //PhotonNetwork.PlayerList[]:배열로 하나 하나 접근
    //PhotonNetwork.CurrentRoom.Name: 현재 방 이름
    //PhotonNetwork.CurrentRoom.PlayerCount: 방에 있는 사람 수
    //PhotonNetwork.CurrentRoom.MaxPlayers: 방 최대 사람 수
    #endregion
    //----------
    public void callAuthManager(bool isMaximize)//저장하기 인듯
    {
        if (isMaximize) 
        {
            AuthManager.Instance.originAchievements.Arr[0] = 0;
            AuthManager.Instance.originAchievements.Arr[1] = 0;
            AuthManager.Instance.originAchievements.Arr[2] = 0;
        }
        if (AuthManager.Instance.User != null) 
        {
            AuthManager.Instance.SaveJson();
            AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
        } 
        else {
            AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
            Debug.Log("LobbyError"); 
        }
    }
}
