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
        RoomRenewal();//�� ����
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "�� / " + PhotonNetwork.CurrentRoom.MaxPlayers + "�ִ�";
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        RoomRenewal();
        //PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        //������ ó�� ���� ��ο��� ���̳� ��
        ChatRPC("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RoomRenewal();
        //PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
        //������ ó�� ���� ��ο��� ���̳� ��
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    //PhotonNetwork.PlayerList[]:�迭�� �ϳ� �ϳ� ����
    //PhotonNetwork.CurrentRoom.Name: ���� �� �̸�
    //PhotonNetwork.CurrentRoom.PlayerCount: �濡 �ִ� ��� ��
    //PhotonNetwork.CurrentRoom.MaxPlayers: �� �ִ� ��� ��
    public void LeaveRoom() 
    {
        Debug.Log("LeaveRoom");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }
    #region ä��
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;//����
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;//��ĭ�� �ø�
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion

}
