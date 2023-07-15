using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChatManager : MonoBehaviourPunCallbacks
{
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    public PhotonView PV;

    private void Awake()
    {
        RoomRenewal();//방 갱신
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        RoomRenewal();
        //PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        //위에거 처럼 쓰면 모두에게 보이나 봄
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RoomRenewal();
        //PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        //위에거 처럼 쓰면 모두에게 보이나 봄
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    //PhotonNetwork.PlayerList[]:배열로 하나 하나 접근
    //PhotonNetwork.CurrentRoom.Name: 현재 방 이름
    //PhotonNetwork.CurrentRoom.PlayerCount: 방에 있는 사람 수
    //PhotonNetwork.CurrentRoom.MaxPlayers: 방 최대 사람 수
    public void LeaveRoom() 
    {
        Debug.Log("LeaveRoom");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }
    #region 채팅
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;//들어가짐
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;//한칸씩 올림
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion

}
