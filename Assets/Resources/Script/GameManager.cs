using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//using static UnityEditor.Progress;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region �̱���
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }
    #endregion

    //������Ʈ Ǯ��
    string[] resourceNames = { "Dummy", "PlayerBulletA", "EnemyA", "Bars", "EnemyB", "EnemyBulletA", "EnemyC", "PlayerName"};   
    List<GameObject>[] pools;//������ �ּҰ� ����� ��

    [Header("UI ����")]
    public GameObject canvas;
    public GameObject chapterArea;
    [Header("�� ����")]
    public GameObject Bars;//ü�� ��
    BoxCollider absoluteAttack;//���� ���� ����
    
    [Header("�÷��̾� ����")]
    public GameObject playerGroup; //�÷��̾ ������ �θ�
    public GameObject playerName;//�÷��̾� �̸�
    [Header("�������� ����")]
    public int curStage = 1;
    public int EneniesCount;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        PhotonNetwork.SendRate = 90;//����ȭ �ӵ�
        PhotonNetwork.SerializationRate = 45;
        absoluteAttack = GetComponent<BoxCollider>();

        //������Ʈ Ǯ��_Ǯ �迭 �ʱ�ȭ
        pools = new List<GameObject>[resourceNames.Length];
        for (int index = 0; index < pools.Length; index++)//Ǯ �ϳ��ϳ� �ʱ�ȭ
            pools[index] = new List<GameObject>();
    }

    #region ������Ʈ Ǯ��
    public GameObject Get(string name) //������ �� �θ���, ������ ����
    {
        int index = 0;
        for (int i = 0; i < resourceNames.Length; i++) 
        {
            if (string.Equals(resourceNames[i], name)) 
            {
                index = i;
                break;
            }
        }
       
        GameObject select = null;
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                if (item.gameObject.tag == "PlayerAttack" || item.gameObject.tag == "EnemyAttack")//�Ѿ��� �Ѿ˿��� ��ȯ
                    select = item.GetComponent<Bullet>().Allreturn();

                else if (item.gameObject.tag == "Enemy")//���� ������ ��ȯ
                    select = item.GetComponent<Enemy>().Allreturn();

                else if (item.gameObject.tag == "UI")//UI�� ��� �׳� ����
                {
                    select = item;
                }
                break;
            }
        }
        //������ �����ϰ� select�� �Ҵ�
        if (!select)
        {
            if (name == "Bars")//�� ü�� �� 
            {

                select = Instantiate(Bars, transform);
                select.transform.parent = canvas.transform;
            }
            else if (name == "PlayerName") //�÷��̾� �̸�
            {
                select = Instantiate(playerName, transform);
                select.transform.parent = canvas.transform;
            }
            else
            {
                select = PhotonNetwork.Instantiate(resourceNames[index], new Vector3(0, 0, 0), Quaternion.identity);
                select.transform.parent = this.transform;

            }
            pools[index].Add(select);
        }
        return select;
    }
    #endregion

    #region �÷��̾� ���� ��� ��, Ư�� ��Ȱ
    public IEnumerator AbsoluteRevive(int size)
    {  
        //�÷��̾� 1, 2�� ��Ȱ
        for (int i = 0; i < size; i++)
            playerGroup.transform.GetChild(i).gameObject.GetComponent<ClickMove>().Revive();
        //���� ���� ǥ��
        chapterArea.SetActive(true);

        //���� ����
        absoluteAttack.enabled = true;
        yield return null;
        absoluteAttack.enabled = false;   
    }
    #endregion 
    private void Start()
    {
        #region ���� �� �÷��̾� ����
        GameObject minePlayer = PhotonNetwork.Instantiate("MinePlayer", new Vector3(0, 2.5f, 0), Quaternion.identity);
        minePlayer.transform.parent = playerGroup.transform;//���̶�Ű â���� �ڽ�����
        minePlayer.transform.position = playerGroup.transform.position;//�����ϴ� ������ ��ġ�� �̵�
        #endregion
    }

    public void SpawnEnemy(string str, Vector3 vec) 
    {
        //�� ��ȯ
        GameObject enemy = Get(str);
        //��ȯ �� ����
        EneniesCount++;
        //��ȯ ��ġ ����
        enemy.transform.position = vec;
    }

    #region ���� ������ ��ü �� ����
    public void EneniesCountControl() 
    {     
        if (--EneniesCount <= 0 ) NextStage();
        Debug.Log("���� ��: " + EneniesCount);
    }
    #endregion

    public void NextStage()
    {
        curStage++;
        int size = playerGroup.transform.childCount;
        //1 �� �� �� �÷��̾� �ʱ�ȭ
        playerGroup.transform.GetChild(0).gameObject.GetComponent<ClickMove>().Revive(); ;
        if (size == 2)//2 �� �� ��  �÷��̾� �ʱ�ȭ 
            playerGroup.transform.GetChild(1).gameObject.GetComponent<ClickMove>().Revive(); ;

        //�ٴڿ� ���� ���� ����
        chapterArea.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Stage " + curStage + '\n' + "Game Start";
    }

    public void EnterStage(Collider other) 
    {
        //���� �ʱ�ȭ(ȣ��Ʈ��)
        EneniesCount = 0;
        //���� ���� ��Ȱ��ȭ
        chapterArea.SetActive(false);
        //�� ��ȯ
        if (curStage == 1)
        {
            SpawnEnemy("EnemyA", new Vector3(5, 0.5f, 5));
            SpawnEnemy("EnemyA", new Vector3(5, 0.5f, 5));
        }
        else if (curStage == 2) 
        {
            SpawnEnemy("EnemyA", new Vector3(5, 0.5f, 5));
            SpawnEnemy("EnemyB", new Vector3(7, 0.5f, 5));
        }
        else if (curStage == 3) SpawnEnemy("EnemyC" , new Vector3(5, 0.5f, 5));
        else if (curStage == 4)
        {
            SpawnEnemy("EnemyB", new Vector3(3, 0.5f, 3));
            SpawnEnemy("EnemyB", new Vector3(3, 0.5f, -3));
        }
        else if (curStage == 5)
        {
            SpawnEnemy("EnemyA", new Vector3(0, 0.5f, 0));
            SpawnEnemy("EnemyB", new Vector3(3, 0.5f, 3));
            SpawnEnemy("EnemyB", new Vector3(3, 0.5f, -3));
        }
    }
}
