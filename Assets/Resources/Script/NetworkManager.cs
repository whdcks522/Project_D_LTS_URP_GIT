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


    #region �渮��Ʈ ����
    // ����ư -2 , ����ư -1 , �� ����
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
        
        // �ִ�������
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // ����, ������ư
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // �������� �´� ����Ʈ ����
        multiple = (currentPage - 1) * CellBtn.Length;//�� �������� ù ��° ���� �ε���
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)//�������� �ڵ����� ���� ����� �������� �װ��ΰ� ��
    {
        //�Ʒ��� �κ� ������ ������ ����Ʈ �ʱ�ȭ�ؼ� ������
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)//���� �����ϴ� ���̶��
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]); //�ٵ� �����ϰ� ���� �ʴٸ�, ���Ѵ�
                else myList[myList.IndexOf(roomList[i])] = roomList[i];     //���� ���� �ε����� ������ ����ȭ(�ο��� �ٲ��� ����ȭ)  
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();

    }
    #endregion

    #region ��������
    void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
            //��Ʈ��ũ ��ü���� �뿡 �ִ� �ο� ���� �κ� �ִ� �ο� �� ����
        LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "�κ� / " + PhotonNetwork.CountOfPlayers + "����";
    }

    private void Start()
    {
        Connect();
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);

    }


    public void Connect() => PhotonNetwork.ConnectUsingSettings();//AuthManager���� �̹� �Ἥ �ʿ� ����

    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();//AuthManager���� �̹� �Ἥ �ʿ� ����

    public override void OnJoinedLobby()//�κ� ������ �г��� ����
    {
        if (AuthManager.Instance.User != null)
             PhotonNetwork.LocalPlayer.NickName = AuthManager.Instance.playerEmail;
        else 
             PhotonNetwork.LocalPlayer.NickName = "NickName" + Random.Range(0, 10000);//NickNameInput.text
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "�� ȯ���մϴ�";
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

    #region ��
    //**********************************�� �̸��� ����ִٸ�
    public void CreateRoom() //�� ����
    {
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
        PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 2 });//������
    }
    public void JoinRandomRoom()//���� �� ����
    {
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);
        PhotonNetwork.JoinRandomRoom();
    } 
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }//���� �̸��� ���� ����� ����
    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }//���� �̸��� ���� ����� ����

    //PhotonNetwork.PlayerList[]:�迭�� �ϳ� �ϳ� ����
    //PhotonNetwork.CurrentRoom.Name: ���� �� �̸�
    //PhotonNetwork.CurrentRoom.PlayerCount: �濡 �ִ� ��� ��
    //PhotonNetwork.CurrentRoom.MaxPlayers: �� �ִ� ��� ��
    #endregion
    //----------
    public void callAuthManager(bool isMaximize)//�����ϱ� �ε�
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
