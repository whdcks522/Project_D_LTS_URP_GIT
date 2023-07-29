using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TmpManager : MonoBehaviourPunCallbacks//�Ϲ����� MonoBehaviour�� �޸� ����.pun�� �̺�Ʈ�� ����
{
    private readonly string gameVersion = "1";//���� �����̶� ������ �ٸ��� ��Ī�� �ȵž� ��
    //������ ������ ������ ���� ���𰡸� ����
    public Text stateText;
    public GameObject fireImage;
    public Button joinButton;
    int State = 1;
    AudioManager audioManager;
    private void Awake()
    {
        audioManager = AuthManager.Instance.GetComponent<AudioManager>();
    }

    public override void OnConnectedToMaster()//���� �����ϸ� //�ڵ����� �����
    {
        if (State == 1) 
        {
            stateText.text = "2�¶���: ������ ������ ���� ��";
            //�÷��̾� �г���
            PhotonNetwork.LocalPlayer.NickName = "NickName" + Random.Range(0, 10000);//NickNameInput.text
            State = 2;                                                          //�ڵ� ������ ���� ���� ��ȯ

            OnConnect();
        }
        
    }

    public override void OnDisconnected(DisconnectCause cause)//���� �����߰ų�, �̹� ���� ���ε� ���� //�ڵ� �����
    {
        if (State == 2) 
        {
            //joinButton.interactable = false;
            stateText.text = $"3��������: ���� ������: {cause.ToString()}";
            //��õ�
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnConnect()//���� ��ư �������� ��
    {
        fireImage.SetActive(true);

        if (State == 1)
        {
            audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);

            //�κ� �����԰� ���ÿ� ������ ����(=���� Ŭ���� ����, ��ġ����ŷ�� ����)�� ���� �õ�
            PhotonNetwork.GameVersion = gameVersion;//���� ����
            PhotonNetwork.ConnectUsingSettings();//���� ����(ex) ���� ���� ��(�̹����� ���� ������ ����))�� ���� ������ ������ ���� �õ�----------->
            
            joinButton.interactable = false;
            stateText.text = "1������ ������ ���� ��..";
        }
        else if (State == 2) 
        {
            //joinButton.interactable = false;
            if (PhotonNetwork.IsConnected) //������ ���� ���� ���� �������, ���� ��ġ��
            {
                stateText.text = "4 ������ �濡 ���� �õ�";
                PhotonNetwork.JoinRandomRoom();//������ �뿡 ���� �õ�(�� ���� ���ٸ� �翬�� ����)------------>
            }
            else //���� �Ұ��� ���
            {
                stateText.text = "3��������: ���� ������: ��õ� �غ���}";
                //��õ�
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)//��κ� ���� �濡 �����µ� ���� ��� ����
    {
        if (State == 2) 
        {

            //���� ���� �����, �ڽ��� ������ ��
            stateText.text = "5 �� ���� �����Ƿ�, ���� ����";
            //����(�� �̸�, ����(�ִ� 2��))
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });//��Ʈ��ũ �󿡼� ���ο� �� ���� ��, ��----------->
        }
    }

    public override void OnJoinedRoom()//�뿡 ���� �Ϸ�� �ڵ� ����(�� ���ӿ� �����ϰų�, ���� ���� ����� ���)
    {
        audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);

        stateText.text = "6 �濡 ����";
        joinButton.interactable = true;
        //�� �Ŵ����� �̵��ϸ� ���� �Ѿ��, �ٸ� ����� ���� �ȳѾ(���� �Ἥ, ����ȭ�� �ȵ�)
        PhotonNetwork.LoadLevel("TmpScene");//������ �ϸ� �������� �ڵ����� ������, ����ȭ�� �ڵ����� ��
    }

    //å���� �̵�
    public void LoadBook() 
    {
        SceneManager.LoadScene("BookScene");
    }
}
