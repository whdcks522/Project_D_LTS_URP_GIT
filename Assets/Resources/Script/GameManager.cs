using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static AuthManager;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

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
    string[] resourceNames = { "Dummy", "PlayerBulletA", "EnemyA", "Bars", "EnemyB", "EnemyBulletA", "EnemyC", 
        "BossA", "EnemyBulletB", "BossB", "EnemyBulletC", "EnemyBulletD"};
    List<GameObject>[] pools;//������ �ּҰ� ����� ��

    [Header("�� ����")]
    public GameObject Bars;//ü�� ��
    BoxCollider absoluteAttack;//���� ���� ����
    
    [Header("�÷��̾� ����")]
    public GameObject playerGroup; //�÷��̾ ������ �θ�
    [Header("�������� ����")]
    public GameObject canvas;//ĵ����
    public GameObject chapterArea;//������ ���� ����

    public Transform[] generatePos;//���� ���� �� ��ġ
    public string mouseText;//�㰡 ����� �ؽ�Ʈ
    public int curStage = 0;//���� ��������
    public int EnemiesCount;//���� ���� ��
    public int MaxEnemiesCount;
    
    public Mouse mouse;
    PhotonView photonView;
    bool isAllreadyAbs;
    public bool isChat;
    public AudioManager audioManager;
    //��Ʈ��ũ ��� ����
    [Header("���� ����")]
    bool archiveUnDead = true;//é�� ��, �� ���� ���� ���� ��(������ false)(0)
    public bool archiveNoShot = true;//é�� ��, �� �� �ߵ� �߻����� ����(��� false)(1)
    //�� ����
    bool isTmpScene;
    enum EnemyType {Dummy, EnemyA, EnemyB, BossA, EnemyC, BossB}
    #region �� ���� Ŭ����
    [Serializable]
    public class EnemySpawnInfo
    {
        public string enemyType;
        public int generateIndex;

        public EnemySpawnInfo(string type, int index)
        {
            enemyType = type;
            generateIndex = index;
        }
    }

    [Serializable]
    public class EnemySpawnInfoArray
    {
        public EnemySpawnInfo[] enemySpawnInfo;
    }

    private List<EnemySpawnInfo> enemySpawnList;//�̹� ������������ ��ȯ�� ���� ���
    public EnemySpawnInfoArray[] enemySpawnInfoArray;//é�� ��ü���� ��ȯ�� ���� ���

    #endregion

    private void Awake()
    {
        //������
        Application.targetFrameRate = 60;
        //����ȭ �ӵ�
        PhotonNetwork.SendRate = 480;//����ȭ �ӵ� = 60
        PhotonNetwork.SerializationRate = 240;//30

        absoluteAttack = GetComponent<BoxCollider>();

        //������Ʈ Ǯ��_Ǯ �迭 �ʱ�ȭ
        pools = new List<GameObject>[resourceNames.Length];
        for (int index = 0; index < pools.Length; index++)//Ǯ �ϳ��ϳ� �ʱ�ȭ
            pools[index] = new List<GameObject>();
        //���� ��
        photonView = GetComponent<PhotonView>();
        //��� ���� �ʱ�ȭ
        audioManager = AuthManager.Instance.GetComponent<AudioManager>();

        //�� ��� ����Ʈ �ʱ�ȭ
        enemySpawnList = new List<EnemySpawnInfo>();
        //�� ��� Ȯ��
        enemySpawnListControl();

        //tmpScene���� Ȯ���ϴ� �Ұ�
        isTmpScene = SceneManager.GetActiveScene().name == "TmpScene";
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "BookScene")
            return;

        #region ���� �� �÷��̾� ����
        GameObject minePlayer = PhotonNetwork.Instantiate("MinePlayer", new Vector3(0, 2.5f, 0), Quaternion.identity);
        //���̶�Ű â���� �ڽ�����
        minePlayer.transform.parent = playerGroup.transform;
        //�����ϴ� ������ ��ġ�� �̵�
        if (photonView.IsMine)
            minePlayer.transform.position = playerGroup.transform.position + new Vector3(0, 0, 2);
        else
            minePlayer.transform.position = playerGroup.transform.position + new Vector3(0, 0, -2);
        #endregion

        //���� ���
        audioManager.PlayBgm(AudioManager.Bgm.Entrance);

        if (!photonView.IsMine) //�� �ڷ� �������
            return;
        

            //������Ʈ Ǯ���� ���� �� ��ü ���
            Dictionary<string, int> enemyMap = new Dictionary<string, int>();

        foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType)))//��ü �ʿ� �� Ÿ�� ���� 0���� ����
        {
            enemyMap[enemyType.ToString()] = 0;
        }

        for (int index = 0; index < enemySpawnInfoArray.Length; index++) 
        {
            Dictionary<string, int> tmpEnemyMap = new Dictionary<string, int>();
            //�о��
            foreach (EnemySpawnInfo spawnInfo in enemySpawnInfoArray[index].enemySpawnInfo)
            {
                if (tmpEnemyMap.ContainsKey(spawnInfo.enemyType))
                {
                    tmpEnemyMap[spawnInfo.enemyType] += 1;
                }
                else
                {
                    tmpEnemyMap[spawnInfo.enemyType] = 1;
                }
            }
            //�����ϱ�
            foreach (KeyValuePair<string, int> kvp in tmpEnemyMap)
            {
                enemyMap[kvp.Key] = Math.Max(kvp.Value, enemyMap[kvp.Key]);
            }
        }

        foreach (KeyValuePair<string, int> kvp in enemyMap)
        {
            Debug.Log("Ű: " + kvp.Key + ", ��: " + kvp.Value);
            for(int i = 0; i < kvp.Value; i++) 
            {
                //�� �̸� ����
                int index = NametoIndex(kvp.Key);
                GameObject enemy = PhotonNetwork.Instantiate(resourceNames[index], new Vector3(0, 0, 0), Quaternion.identity);
                pools[index].Add(enemy);

                enemy.GetComponent<Enemy>().photonView.RPC("RPCfirstInstantaite", RpcTarget.AllBuffered);
            }
        }
    }

    int NametoIndex(string _name) //������ƮǮ������ �����ϴ� ���ڿ��� ������ ��ȯ
    {
        for(int i = 0; i < resourceNames.Length; i++) 
        {
            if (string.Equals(resourceNames[i], _name))
            {
                return i;
            }
        }
        return 0;
    }

    #region ������Ʈ Ǯ��
    public GameObject Get(string name) //������ �� �θ���, ������ ����
    {
        int index = NametoIndex(name);

        GameObject select = null;
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                if (item.gameObject.tag == "PlayerAttack" || item.gameObject.tag == "EnemyAttack")//�Ѿ��� �Ѿ˿��� ��ȯ
                {
                    select = item;
                }

                else if (item.gameObject.tag == "Enemy")//���� ������ ��ȯ
                {
                    select = item;
                }
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
            else
            {
                select = PhotonNetwork.Instantiate(resourceNames[index], new Vector3(0, 0, 0), Quaternion.identity);
            }
            pools[index].Add(select);
        }
        return select;
    }
    #endregion

    [PunRPC]
    public void AbsoluteReviveStart() 
    {
        //����
        archiveUnDead = false;
        //�������
        audioManager.PlayBgm(AudioManager.Bgm.Entrance);

        StartCoroutine(AbsoluteRevive());
    } 
    #region �÷��̾� ���� ��� ��, Ư�� ��Ȱ
    public IEnumerator AbsoluteRevive()
    {
        if (!isAllreadyAbs) 
        {
            //�÷��̾� 1, 2�� ��Ȱ
            for (int i = 0; i < playerGroup.transform.childCount; i++)
                playerGroup.transform.GetChild(i).gameObject.GetComponent<ClickMove>().Revive();
            //���� ���� ǥ��
            chapterArea.SetActive(true);
            //�� ���̵���
            mouse.VisibleDissolve();
            //���� ����
            absoluteAttack.enabled = true;
            yield return new WaitForSeconds(0.5f);
            absoluteAttack.enabled = false;

            isAllreadyAbs = true;
        }      
    }
    #endregion 

    public void SpawnEnemy(string str, int generateIndex) //�� �� ������
    {
        //��ȯ �� ����
        EnemiesCount++;
        if (photonView.IsMine) 
        {
            //�� ��ȯ
            GameObject enemy = Get(str);
            enemy.GetComponent<Enemy>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, generatePos[generateIndex - 1].position);

            #region useNav�� ���� ��� ��ġ �����ε� �Ⱦ�
            //Enemy enemyScript = enemy.GetComponent<Enemy>();
            //if (enemyScript.isUseNav) 
            {
                //�� �� �Ƚᵵ �۵��ǳ�
                //enemyScript.photonView.RPC("OriginControlStart", RpcTarget.AllBuffered, enemy.transform.position);
                // StartCoroutine(enemyScript.OriginConrol(enemy.transform.position));
            }
            #endregion
        }
    }

    #region ���� ������ ��ü �� ����
    public void EneniesCountControl() //�� �� �����
    {
        EnemiesCount--;

        if (EnemiesCount <= 0 && photonView.IsMine)
                photonView.RPC("NextStage", RpcTarget.AllBuffered);
        //NextStage();
    }
    #endregion

    #region ���� �������� Ŭ�����ؼ� ���� ����������
    [PunRPC]
    public void NextStage()//������ �� �� �Ҹ�
    {
        //���� ����������
        if (!isTmpScene)
            curStage++;

        if (curStage == enemySpawnInfoArray.Length && !isTmpScene)//é�� �� Ŭ����
        {
                //é�� ��, �� ���� ���� ���� ��(���� 0)
                if(archiveUnDead)
                    AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.Undead] = 1;

                //é�� ��, �� �� �ߵ� �߻����� ����(��� false)(���� 1)
                if (archiveNoShot)
                    AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.NoShot] = 1;

                //é�� 1 Ŭ����(���� 2)
                if (SceneManager.GetActiveScene().name == "Chap1_Scene")
                    AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.Chapter1] = 1;

                //é�� 2 Ŭ����(���� 3)
                if (SceneManager.GetActiveScene().name == "Chap2_Scene")
                    AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.Chapter2] = 1;

                //���� ��ü ����
                AuthManager.Instance.SaveJson();

                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("LobbyScene");
        }
        else //�߰� ��� Ŭ���� ��
        {
            //�÷��̾� ����
            int size = playerGroup.transform.childCount;//�÷��̾��� ��
            //n �� �� �� �÷��̾� �ʱ�ȭ
            for (int i = 0; i < playerGroup.transform.childCount; i++)
                playerGroup.transform.GetChild(i).gameObject.GetComponent<ClickMove>().Revive(); ;

            //�ٴڿ� ���� ���� ����
            chapterArea.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Stage " + curStage + '\n' + "Game Start";
            // �� ���̵���
            mouse.VisibleDissolve();
            //�� ����Ʈ �ʱ�ȭ
            enemySpawnListControl();
            //���� �Ҹ�
            audioManager.PlayBgm(AudioManager.Bgm.Entrance);
            //���� ȿ����
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);

            //�� ����
            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
            //roomProperties.Add("IsAllowedToEnter", false);//������ �� ó�� ���� ���̻� ������
            roomProperties.Add("IsAllowedToExit", true);//��� �߿��� ���� �� �ֵ���
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);//��ο��� �����-------
        }  
    }
    #endregion


    #region �������� ����
    [PunRPC]
    public void EnterStage() //�� �� ������
    {
        //���� �ʱ�ȭ(ȣ��Ʈ��)
        EnemiesCount = 0;
        isAllreadyAbs = false;
        //���� ���� ��Ȱ��ȭ
        chapterArea.SetActive(false);
        //�� ��Ȱ��ȭ
        mouse.InvisibleDissolve();
        //�� UI ����
        mouse.isMaxfalse();
        //���� ȿ����
        audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);

        //�� ��ȯ, tmpScene�� ���, ��Ӵٿ��� ����
        if (isTmpScene)
        {
            SpawnEnemy(playerGroup.GetComponent<TrainManager>().DropDownTranslate(), 6);
        }
        else //é���� ���, ����Ʈ�� ����
        {
            //�� ��ȯ
            {
                foreach (var spawnInfo in enemySpawnList)
                    SpawnEnemy(spawnInfo.enemyType, spawnInfo.generateIndex);
            }
        }
        //���� ��, ���� �Ұ��� �ϵ��� �� ����
        ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();

        if (curStage != enemySpawnInfoArray.Length - 1)//�Ϲ� ������ ���
        {
            if (curStage == 0)//ù ���� �� ���
            {
                roomProperties.Add("IsAllowedToEnter", false);//--------------
            }

            if(SceneManager.GetActiveScene().name == "Chap1_Scene")//é�� 1 �Ϲ� �뷡
                        audioManager.PlayBgm(AudioManager.Bgm.Chapter1);
            else if (SceneManager.GetActiveScene().name == "Chap2_Scene")//é�� 1 �Ϲ� �뷡
                audioManager.PlayBgm(AudioManager.Bgm.Chapter2);
        }
        else  //���� ������ ���
        {
            if (SceneManager.GetActiveScene().name == "Chap1_Scene")//é�� 1 ���� �뷡
                audioManager.PlayBgm(AudioManager.Bgm.Chapter1_BossA);
            else if (SceneManager.GetActiveScene().name == "Chap2_Scene")//é�� 2 ���� �뷡
                audioManager.PlayBgm(AudioManager.Bgm.Chapter2_BossB);
        }
        roomProperties.Add("IsAllowedToExit", false);//���� �� ����������
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);    
    }
    #endregion

    #region �� ��� ����Ʈ �ʱ�ȭ
    void enemySpawnListControl()
    {
        enemySpawnList.Clear();
        mouseText = "";
        //�������� �� ����Ʈ�� ����
        foreach (EnemySpawnInfo spawnInfo in enemySpawnInfoArray[curStage].enemySpawnInfo)
            enemySpawnList.Add(spawnInfo);
       
        #region �� ��� ����
        string curEnemyType = "";
        int curEnemyTypeCount = 0;
        MaxEnemiesCount = enemySpawnList.Count;

        foreach (var spawnInfo in enemySpawnList)
        {
            if (curEnemyType == spawnInfo.enemyType)//���� ������ ���
            {
                curEnemyTypeCount++;
            }
            else
            {
                //�� ó�� ��縦 �����ϴ� ���� �ƴ϶�� ,�ϰ� ü �߰�
                if (curEnemyType != "")
                {
                    mouseText += ", " + curEnemyType + " " + curEnemyTypeCount + "ü";
                }
                //�ش� Ÿ�� ����
                curEnemyType = spawnInfo.enemyType;
                //�ش� Ÿ���� �� �� �ʱ�ȭ
                curEnemyTypeCount = 1;
            }
        }

        // ������ �� ���� ������ �߰�
        if (curEnemyType != "")
        {
            mouseText += ", " + curEnemyType + " " + curEnemyTypeCount + "ü";
        }

        // ù ��° ��ǥ�� ���� ����
        if (mouseText.Length > 2 && mouseText.Substring(0, 2) == ", ")
        {
            mouseText = mouseText.Substring(2);
        }
        #endregion
    }
    #endregion
}
