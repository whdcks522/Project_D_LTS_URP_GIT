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
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    public PhotonView PV;
    GameManager gameManager;

    private void Awake()
    {
        RoomRenewal();//�� ����
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";

        gameManager = GameManager.Instance;
    }
    [PunRPC]
    public void RoomRenewal()
    {
        ListText.text = "";//������ : 
        RoomInfoText.text = "";//�� ���� : 
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text += PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "�� / " + " �ִ� " + PhotonNetwork.CurrentRoom.MaxPlayers + "��";
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=red>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=red>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");//yellow
    }

    //PhotonNetwork.PlayerList[]:�迭�� �ϳ� �ϳ� ����
    //PhotonNetwork.CurrentRoom.Name: ���� �� �̸�
    //PhotonNetwork.CurrentRoom.PlayerCount: �濡 �ִ� ��� ��
    //PhotonNetwork.CurrentRoom.MaxPlayers: �� �ִ� ��� ��

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return))//����Ű ���� ��
        {
            if (!gameManager.isChat) //����
            {
                gameManager.isChat = true;
                ChatInput.Select();
                ChatInput.ActivateInputField();
            }
            else //�ݱ�
            {
                gameManager.isChat = false;
                ChatInput.DeactivateInputField();

                PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
                ChatInput.text = "";
            }
        }
    }
    //GenericPropertyJSON:{"name":"enemySpawnInfoArray","type":-1,"arraySize":5,"arrayType":"EnemySpawnInfoArray","children":[{"name":"Array","type":-1,"arraySize":5,"arrayType":"EnemySpawnInfoArray","children":[{"name":"size","type":12,"val":5},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":2},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"Dummy"},{"name":"generateIndex","type":0,"val":5}]},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"Dummy"},{"name":"generateIndex","type":0,"val":6}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":1},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"EnemyA"},{"name":"generateIndex","type":0,"val":6}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":2,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":2},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"EnemyB"},{"name":"generateIndex","type":0,"val":2}]},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"EnemyB"},{"name":"generateIndex","type":0,"val":8}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":1},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"BossC"},{"name":"generateIndex","type":0,"val":6}]}]}]}]},{"name":"data","type":-1,"children":[{"name":"enemySpawnInfo","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"Array","type":-1,"arraySize":1,"arrayType":"EnemySpawnInfo","children":[{"name":"size","type":12,"val":1},{"name":"data","type":-1,"children":[{"name":"enemyType","type":3,"val":"BossA"},{"name":"generateIndex","type":0,"val":5}]}]}]}]}]}]}

    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
        photonView.RPC("RoomRenewal", RpcTarget.AllBuffered);
    }

    #region ä��

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
