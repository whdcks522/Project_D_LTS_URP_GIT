using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class TmpManager : MonoBehaviourPunCallbacks//�Ϲ����� MonoBehaviour�� �޸� ����.pun�� �̺�Ʈ�� ����
{
    private readonly string gameVersion = "1";//���� �����̶� ������ �ٸ��� ��Ī�� �ȵž� ��
    //������ ������ ������ ���� ���𰡸� ����
    public Text connecettionInfoText;
    public Button joinButton;
    public int State = 1;
    
    public override void OnConnectedToMaster()//���� �����ϸ� //�ڵ����� �����
    {
        Debug.Log("B");
        if (State == 1) 
        {
            
            connecettionInfoText.text = "2�¶���: ������ ������ ���� ��";
            //�÷��̾� �г���
            PhotonNetwork.LocalPlayer.NickName = "NickName" + Random.Range(0, 10000);//NickNameInput.text
            State = 2;                                                          //�ڵ� ������ ���� ���� ��ȯ

            OnConnect();
        }
        
    }

    public override void OnDisconnected(DisconnectCause cause)//���� �����߰ų�, �̹� ���� ���ε� ���� //�ڵ� �����
    {
        Debug.Log("C");
        //joinButton.interactable = false;
        connecettionInfoText.text = $"3��������: ���� ������: {cause.ToString()}";
        //��õ�
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnConnect()//���� ��ư �������� ��
    {
        Debug.Log("0");
        if (State == 1)// && !PhotonNetwork.IsConnected
        {
            Debug.Log("A");
            //�κ� �����԰� ���ÿ� ������ ����(=���� Ŭ���� ����, ��ġ����ŷ�� ����)�� ���� �õ�
            PhotonNetwork.GameVersion = gameVersion;//���� ����
            PhotonNetwork.ConnectUsingSettings();//���� ����(ex) ���� ���� ��(�̹����� ���� ������ ����))�� ���� ������ ������ ���� �õ�----------->
            
            joinButton.interactable = false;
            connecettionInfoText.text = "1������ ������ ���� ��..";
        }
        else if (State == 2) 
        {
            Debug.Log("D");
            joinButton.interactable = false;
            if (PhotonNetwork.IsConnected) //������ ���� ���� ���� �������, ���� ��ġ��
            {
                connecettionInfoText.text = "4 ������ �濡 ���� �õ�";
                PhotonNetwork.JoinRandomRoom();//������ �뿡 ���� �õ�(�� ���� ���ٸ� �翬�� ����)------------>
            }
            else //���� �Ұ��� ���
            {
                connecettionInfoText.text = "3��������: ���� ������: ��õ� �غ���}";
                //��õ�
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)//��κ� ���� �濡 �����µ� ���� ��� ����
    {
        Debug.Log("E");
        if (State == 2) 
        {
            
            //���� ���� �����, �ڽ��� ������ ��
            connecettionInfoText.text = "5 �� ���� �����Ƿ�, ���� ����";
            //����(�� �̸�, ����(�ִ� 2��))
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });//��Ʈ��ũ �󿡼� ���ο� �� ���� ��, ��----------->
        }
    }

    public override void OnJoinedRoom()//�뿡 ���� �Ϸ�� �ڵ� ����(�� ���ӿ� �����ϰų�, ���� ���� ����� ���)
    {
        Debug.Log("F");

        connecettionInfoText.text = "6 �濡 ����";
        joinButton.interactable = true;
        //�� �Ŵ����� �̵��ϸ� ���� �Ѿ��, �ٸ� ����� ���� �ȳѾ(���� �Ἥ, ����ȭ�� �ȵ�)
        PhotonNetwork.LoadLevel("TmpScene");//������ �ϸ� �������� �ڵ����� ������, ����ȭ�� �ڵ����� ��
    }
}
