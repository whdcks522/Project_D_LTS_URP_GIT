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
    string[] resourceNames = { "Dummy", "PlayerBulletA", "EnemyA", "Bars", "EnemyB", "EnemyBulletA", "EnemyC", "PlayerName"};   
    List<GameObject>[] pools;//실제로 주소가 저장될 곳

    [Header("UI 관련")]
    public GameObject canvas;
    public GameObject chapterArea;
    [Header("적 관련")]
    public GameObject Bars;//체력 바
    BoxCollider absoluteAttack;//절대 공격 영역
    
    [Header("플레이어 관련")]
    public GameObject playerGroup; //플레이어가 생성될 부모
    public GameObject playerName;//플레이어 이름
    [Header("스테이지 관련")]
    public int curStage = 1;
    public int EneniesCount;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        PhotonNetwork.SendRate = 90;//동기화 속도
        PhotonNetwork.SerializationRate = 45;
        absoluteAttack = GetComponent<BoxCollider>();

        //오브젝트 풀링_풀 배열 초기화
        pools = new List<GameObject>[resourceNames.Length];
        for (int index = 0; index < pools.Length; index++)//풀 하나하나 초기화
            pools[index] = new List<GameObject>();
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
                    select = item.GetComponent<Bullet>().Allreturn();

                else if (item.gameObject.tag == "Enemy")//적은 적에서 반환
                    select = item.GetComponent<Enemy>().Allreturn();

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
            else if (name == "PlayerName") //플레이어 이름
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

    #region 플레이어 전원 사망 시, 특수 부활
    public IEnumerator AbsoluteRevive(int size)
    {  
        //플레이어 1, 2명 부활
        for (int i = 0; i < size; i++)
            playerGroup.transform.GetChild(i).gameObject.GetComponent<ClickMove>().Revive();
        //시작 영역 표시
        chapterArea.SetActive(true);

        //절대 공격
        absoluteAttack.enabled = true;
        yield return null;
        absoluteAttack.enabled = false;   
    }
    #endregion 
    private void Start()
    {
        #region 시작 시 플레이어 생성
        GameObject minePlayer = PhotonNetwork.Instantiate("MinePlayer", new Vector3(0, 2.5f, 0), Quaternion.identity);
        minePlayer.transform.parent = playerGroup.transform;//하이라키 창에서 자식으로
        minePlayer.transform.position = playerGroup.transform.position;//시작하는 물리적 위치를 이동
        #endregion
    }

    public void SpawnEnemy(string str, Vector3 vec) 
    {
        //적 소환
        GameObject enemy = Get(str);
        //소환 수 전달
        EneniesCount++;
        //소환 위치 조정
        enemy.transform.position = vec;
    }

    #region 적을 잡으면 개체 수 감소
    public void EneniesCountControl() 
    {     
        if (--EneniesCount <= 0 ) NextStage();
        Debug.Log("남은 적: " + EneniesCount);
    }
    #endregion

    public void NextStage()
    {
        curStage++;
        int size = playerGroup.transform.childCount;
        //1 명 일 때 플레이어 초기화
        playerGroup.transform.GetChild(0).gameObject.GetComponent<ClickMove>().Revive(); ;
        if (size == 2)//2 명 일 때  플레이어 초기화 
            playerGroup.transform.GetChild(1).gameObject.GetComponent<ClickMove>().Revive(); ;

        //바닥에 적힌 글자 변경
        chapterArea.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "Stage " + curStage + '\n' + "Game Start";
    }

    public void EnterStage(Collider other) 
    {
        //숫자 초기화(호스트만)
        EneniesCount = 0;
        //시작 영역 비활성화
        chapterArea.SetActive(false);
        //적 소환
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
