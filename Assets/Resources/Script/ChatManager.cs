using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class ChatManager : MonoBehaviourPunCallbacks
{
    [Header("채팅")]
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    public PhotonView PV;
    GameManager gameManager;
    AudioManager audioManager;
    //[Header("접속 관리")]


    private void Awake()
    {
        RoomRenewal();//방 갱신
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";

        gameManager = GameManager.Instance;
        audioManager = AuthManager.Instance.GetComponent<AudioManager>();
    }
    [PunRPC]
    public void RoomRenewal()
    {
        ListText.text = "";//참가자 : 
        RoomInfoText.text = "";//방 정보 : 
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text += PhotonNetwork.CurrentRoom.Name + " (" + PhotonNetwork.CurrentRoom.PlayerCount + "/"+ PhotonNetwork.CurrentRoom.MaxPlayers + ")";
    }

    //public override void 

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)//각각인가봄
    {
        bool canEnterRoom = (bool)PhotonNetwork.CurrentRoom.CustomProperties["IsAllowedToEnter"];
        if (canEnterRoom) //게임 시작 전에만 보임
        {
            RoomRenewal();
            ChatRPC(newPlayer.NickName + "님이 참가하셨습니다", "");
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)//각각인가봄
    {
        RoomRenewal();
    }

    //PhotonNetwork.PlayerList[]:배열로 하나 하나 접근
    //PhotonNetwork.CurrentRoom.Name: 현재 방 이름
    //PhotonNetwork.CurrentRoom.PlayerCount: 방에 있는 사람 수
    //PhotonNetwork.CurrentRoom.MaxPlayers: 방 최대 사람 수

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))//엔터키 누를 시
        {
            if (!gameManager.isChat && ChatInput.text == "") //열기
            {
                gameManager.isChat = true;
                ChatInput.Select();
                ChatInput.ActivateInputField();
            }
            else //닫기
            {
                gameManager.isChat = false;
                ChatInput.DeactivateInputField();

                PV.RPC("ChatRPC", RpcTarget.All,  PhotonNetwork.NickName + " : " + ChatInput.text, AuthManager.Instance.playerEmail);
                ChatInput.text = "";
            }
        }
    }
    //GenericPropertyJSON:{"name":"enemySpawnInfoArray","type":-1,"arraySize":5,"arrayType":"EnemySpawnInfoArray","children":[{"name":"Array","type":-1,"arraySize":5,"arrayType":"EnemySpawnInfoArray","children":[{"name":"size","type":12,"val":5},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":2},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"Dummy"},{"name":"generateIndex","type":0,"val":5}]},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"Dummy"},{"name":"generateIndex","type":0,"val":6}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":1},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"EnemyA"},{"name":"generateIndex","type":0,"val":6}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":2},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"EnemyB"},{"name":"generateIndex","type":0,"val":2}]},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"EnemyB"},{"name":"generateIndex","type":0,"val":8}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":1},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"BossC"},{"name":"generateIndex","type":0,"val":6}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":1},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"BossA"},{"name":"generateIndex","type":0,"val":5}]}]}]}]}]}]}

    public void LeaveRoom() 
    {
        bool canEnterRoom = (bool)PhotonNetwork.CurrentRoom.CustomProperties["IsAllowedToEnter"];
        bool canExitRoom = (bool)PhotonNetwork.CurrentRoom.CustomProperties["IsAllowedToExit"];
        //방장은 무조건 못나감
        if (gameManager.photonView.IsMine)//전투중이 아닐 경우, 방장만 못나감
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
            ChatRPC("방장은 게임 진행 중 나갈 수 없습니다.", "");
        }
        //방장이 아니면 전투 중일 때만 못나감
        else if(!canExitRoom)//전투중이 아닐 경우, 방장만 못나감
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
            ChatRPC("전투 중 나갈 수 없습니다.", "");
        }
        else 
        {
            photonView.RPC("ChatRPC", RpcTarget.AllBuffered, PhotonNetwork.NickName + "님이 퇴장하셨습니다", "");

            Debug.Log("LeaveRoom");

            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("LobbyScene");

           // photonView.RPC("RoomRenewal", RpcTarget.AllBuffered);
            
        }
    }

    #region 채팅

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg, string chatMail)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
        { 
            if (ChatText[i].text == "")
            {
                isInput = true;//들어가짐
                if(chatMail == "")//중립일 경우 흰색
                    ChatText[i].text = "<color=#FFFFFFFF>" + msg + "</color>";
                else if(chatMail == AuthManager.Instance.playerEmail)//자신으로 보일 경우 파란색
                    ChatText[i].text = "<color=#58AFBFFF>" + msg + "</color>";
                else//남으로 보일 경우 빨간색
                    ChatText[i].text = "<color=#9C6359FF>" + msg + "</color>";

                break;
            }
        }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) 
                ChatText[i - 1].text = ChatText[i].text;//한칸씩 올림

            //텅 빈 맨 마지막 갱신
            if (chatMail == "")
                ChatText[ChatText.Length - 1].text = "<color=#FFFFFFFF>" + msg + "</color>";
            else if (chatMail == AuthManager.Instance.playerEmail)
                ChatText[ChatText.Length - 1].text = "<color=#58AFBFFF>" + msg + "</color>";
            else
                ChatText[ChatText.Length - 1].text = "<color=#9C6359FF>" + msg + "</color>";
        }
    }
    #endregion

}
