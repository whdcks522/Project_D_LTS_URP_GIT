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
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region 싱글턴
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
   
    //오브젝트 풀링
    string[] resourceNames = { "Dummy", "PlayerBulletA", "EnemyA", "Bars", "EnemyB", "EnemyBulletA", "EnemyC", 
        "BossA", "EnemyBulletB"};
    List<GameObject>[] pools;//실제로 주소가 저장될 곳

    [Header("적 관련")]
    public GameObject Bars;//체력 바
    BoxCollider absoluteAttack;//절대 공격 영역
    
    [Header("플레이어 관련")]
    public GameObject playerGroup; //플레이어가 생성될 부모
    [Header("스테이지 관련")]
    public GameObject canvas;//캔버스
    public GameObject chapterArea;//닿으면 게임 시작

    public Transform[] generatePos;//적이 생성 될 위치
    public string mouseText;//쥐가 사용할 텍스트
    public int curStage = 0;//현재 스테이지
    public int EnemiesCount;//남은 적의 수
    public int MaxEnemiesCount;
    
    public Mouse mouse;
    PhotonView photonView;
    bool isAllreadyAbs;
    public bool isChat;
    public AudioManager audioManager;
    //네트워크 통신 관련
    [Header("업적 관련")]
    bool archiveUnDead = true;//챕터 중, 한 번도 죽지 않을 것(죽으면 false)(0)
    public bool archiveNoShot = true;//챕터 중, 단 한 발도 발사하지 않음(쏘면 false)(1)
    #region 적 정보 클래스
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

    private List<EnemySpawnInfo> enemySpawnList;//이번 스테이지에서 소환할 적의 목록
    public EnemySpawnInfoArray[] enemySpawnInfoArray;//챕터 전체에서 소환할 적의 목록

    #endregion

    private void Awake()
    {
        //프레임
        Application.targetFrameRate = 60;
        //동기화 속도
        PhotonNetwork.SendRate = 480;//동기화 속도 = 60
        PhotonNetwork.SerializationRate = 240;//30

        absoluteAttack = GetComponent<BoxCollider>();

        //오브젝트 풀링_풀 배열 초기화
        pools = new List<GameObject>[resourceNames.Length];
        for (int index = 0; index < pools.Length; index++)//풀 하나하나 초기화
            pools[index] = new List<GameObject>();
        //포톤 뷰
        photonView = GetComponent<PhotonView>();
        //배경 음악 초기화
        audioManager = AuthManager.Instance.GetComponent<AudioManager>();

        //적 목록 리스트 초기화
        enemySpawnList = new List<EnemySpawnInfo>();
        //적 목록 확인
        enemySpawnListControl();
    }

    private void Start()
    {
        #region 시작 시 플레이어 생성
        GameObject minePlayer = PhotonNetwork.Instantiate("MinePlayer", new Vector3(0, 2.5f, 0), Quaternion.identity);
        //하이라키 창에서 자식으로
        minePlayer.transform.parent = playerGroup.transform;
        //시작하는 물리적 위치를 이동
        minePlayer.transform.position = playerGroup.transform.position + new Vector3(0, 0, Random.Range(-4, 2));
        #endregion
        audioManager.PlayBgm(AudioManager.Bgm.Entrance);
    }


    #region 오브젝트 풀링
    public GameObject Get(string name) //있으면 적 부르고, 없으면 생성
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
                if (item.gameObject.tag == "PlayerAttack" || item.gameObject.tag == "EnemyAttack")//총알은 총알에서 반환
                {
                    select = item;
                }

                else if (item.gameObject.tag == "Enemy")//적은 적에서 반환
                {
                    select = item;
                }
                else if (item.gameObject.tag == "UI")//UI일 경우 그냥 생성
                {
                    select = item;
                }
                break;
            }
        }
        //없으면 생성하고 select에 할당
        if (!select)
        {
            if (name == "Bars")//적 체력 바 
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
        //업적
        archiveUnDead = false;
        //배경음악
        audioManager.PlayBgm(AudioManager.Bgm.Entrance);

        StartCoroutine(AbsoluteRevive());
    } 
    #region 플레이어 전원 사망 시, 특수 부활
    public IEnumerator AbsoluteRevive()
    {
        if (!isAllreadyAbs) 
        {
            //플레이어 1, 2명 부활
            for (int i = 0; i < playerGroup.transform.childCount; i++)
                playerGroup.transform.GetChild(i).gameObject.GetComponent<ClickMove>().Revive();
            //시작 영역 표시
            chapterArea.SetActive(true);
            //쥐 보이도록
            mouse.VisibleDissolve();
            //절대 공격
            absoluteAttack.enabled = true;
            yield return new WaitForSeconds(0.5f);
            absoluteAttack.enabled = false;

            isAllreadyAbs = true;
        }      
    }
    #endregion 

    

    public void SpawnEnemy(string str, int generateIndex) //둘 다 실행함
    {
        //소환 수 전달
        EnemiesCount++;
        if (photonView.IsMine) 
        {
            //적 소환
            GameObject enemy = Get(str);
            enemy.GetComponent<Enemy>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, generatePos[generateIndex - 1].position);

            #region useNav를 쓰는 경우 위치 고정인데 안씀
            //Enemy enemyScript = enemy.GetComponent<Enemy>();
            //if (enemyScript.isUseNav) 
            {
                //왜 또 안써도 작동되냐
                //enemyScript.photonView.RPC("OriginControlStart", RpcTarget.AllBuffered, enemy.transform.position);
                // StartCoroutine(enemyScript.OriginConrol(enemy.transform.position));
            }
            #endregion
        }
    }

    #region 적을 잡으면 개체 수 감소
    public void EneniesCountControl() //둘 다 사용함
    {
        if (--EnemiesCount <= 0)
            NextStage();
    }
    #endregion

    void TmpEnd() 
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("AuthScene");
        PhotonNetwork.Disconnect();
    }

    #region 현재 스테이지 클리어해서 다음 스테이지로
    [PunRPC]
    public void NextStage()//위에서 둘 다 불림
    {
            //다음 스테이지로
            curStage++;
        if (curStage == enemySpawnInfoArray.Length)//챕터 올 클리어
        {
            if (SceneManager.GetActiveScene().name == "TmpScene")//테스트 중이라면 시작 창으로
            {
                Invoke("TmpEnd", 1f);
            }     
            else if (SceneManager.GetActiveScene().name == "GameScene")//게임 중이라면 로비로
            {
                //챕터 중, 한 번도 죽지 않을 것(업적 0)
                if(archiveUnDead)
                    AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.Undead] = 1;

                //챕터 중, 단 한 발도 발사하지 않음(쏘면 false)(업적 1)
                if (archiveNoShot)
                    AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.NoShot] = 1;

                //챕터 1 클리어(업적 2)
                AuthManager.Instance.originAchievements.Arr[(int)ArchiveType.Chapter1] = 1;


                //업적 저장
                AuthManager.Instance.SaveJson();

                //로비로 이동
                Debug.Log("LeaveRoom");
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }
        else 
        {
            //플레이어 관리
            int size = playerGroup.transform.childCount;//플레이어의 수
            //n 명 일 때 플레이어 초기화
            for (int i = 0; i < playerGroup.transform.childCount; i++)
                playerGroup.transform.GetChild(i).gameObject.GetComponent<ClickMove>().Revive(); ;

            //바닥에 적힌 글자 변경
            chapterArea.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Stage " + curStage + '\n' + "Game Start";
            // 쥐 보이도록
            mouse.VisibleDissolve();
            //적 리스트 초기화
            enemySpawnListControl();
            //대기실 소리
            audioManager.PlayBgm(AudioManager.Bgm.Entrance);
            //퇴장 효과음
            audioManager.PlaySfx(AudioManager.Sfx.DoorDrag, true);
        }  
    }
    #endregion


    #region 스테이지 시작
    [PunRPC]
    public void EnterStage() //둘 다 실행함
    {
        //숫자 초기화(호스트만)
        EnemiesCount = 0;
        isAllreadyAbs = false;
        //시작 영역 비활성화
        chapterArea.SetActive(false);
        //쥐 비활성화
        mouse.InvisibleDissolve();
        //쥐 UI 종료
        mouse.isMaxfalse();


        if (curStage != enemySpawnInfoArray.Length - 1)//일반 전투
        {
            if (curStage == 0)//첫 전투 일 경우
            {
                //ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
                //roomProperties.Add("IsAllowedToEnter", false);
                //PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
            }

            audioManager.PlayBgm(AudioManager.Bgm.Chapter1);
        }
        else  //보스 전투
        {
            audioManager.PlayBgm(AudioManager.Bgm.Chapter1_BossA);
        }
        
        //입장 효과음
        audioManager.PlaySfx(AudioManager.Sfx.DoorOpen, true);

        //적 소환
        {
            foreach (var spawnInfo in enemySpawnList)
                SpawnEnemy(spawnInfo.enemyType, spawnInfo.generateIndex);
        }     
    }
    #endregion

    #region 적 목록 리스트 초기화
    void enemySpawnListControl()
    {
        enemySpawnList.Clear();
        mouseText = "";
        //스테이지 적 리스트에 삽입
        foreach (EnemySpawnInfo spawnInfo in enemySpawnInfoArray[curStage].enemySpawnInfo)
            enemySpawnList.Add(spawnInfo);
       
        #region 쥐 대사 관리
        string curEnemyType = "";
        int curEnemyTypeCount = 0;
        MaxEnemiesCount = enemySpawnList.Count;

        foreach (var spawnInfo in enemySpawnList)
        {
            if (curEnemyType == spawnInfo.enemyType)//같은 종류인 경우
            {
                curEnemyTypeCount++;
            }
            else
            {
                //맨 처음 대사를 시작하는 것이 아니라면 ,하고 체 추가
                if (curEnemyType != "")
                {
                    mouseText += ", " + curEnemyType + " " + curEnemyTypeCount + "체";
                }
                //해당 타입 저장
                curEnemyType = spawnInfo.enemyType;
                //해당 타입의 적 수 초기화
                curEnemyTypeCount = 1;
            }
        }

        // 마지막 적 종류 정보를 추가
        if (curEnemyType != "")
        {
            mouseText += ", " + curEnemyType + " " + curEnemyTypeCount + "체";
        }

        // 첫 번째 쉼표와 공백 제거
        if (mouseText.Length > 2 && mouseText.Substring(0, 2) == ", ")
        {
            mouseText = mouseText.Substring(2);
        }
        #endregion
    }
    #endregion
}
