using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ClickMove : MonoBehaviourPunCallbacks
{
    
    Animator anim;
    LineRenderer lr;
    Coroutine draw;
    Rigidbody rigid;
    Ray ray;

    public Transform spot;//이동할 장소
    GameManager gameManager;//게임매니저
    NavMeshAgent agent;
    NavMeshSurface nms;//ai
    PhotonView photonView;//포톤 뷰
    ParticleSystem particle;//파티클 시스템
    CapsuleCollider col;
    //매터리얼 변환
    SkinnedMeshRenderer []skinnedMeshRenderer = new SkinnedMeshRenderer[2];

    [Header("UI")]
    public GameObject playerName;//플레이어 이름
    public GameObject darkThunder;//플레이어 이름
    public GameObject blueThunder;//플레이어 이름
    bool isControl;
    bool isDissolve;
    bool isShot;
    float curTime = 1f;
    float maxTime = 1f;//사격 후 대기시간

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        gameManager = GameManager.Instance;
        
        particle = GetComponent<ParticleSystem>();
        col = GetComponent<CapsuleCollider>();

        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
            spot = gameManager.gameObject.transform.GetChild(0);

        //네비매쉬
        nms = gameManager.GetComponent<NavMeshSurface>();
        //라인 렌더러
        lr = GetComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material.color = new Color(0.3f, 0.7f, 0.3f);
        lr.enabled = false;

        //AI 경로
        nms.BuildNavMesh();

        //매터리얼에 접근
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        playerName.GetComponent<Text>().text = photonView.IsMine? PhotonNetwork.NickName : photonView.Owner.NickName;
        //스킬 바
        if (photonView.IsMine)
        {
            darkThunder.gameObject.SetActive(true);
            darkThunder.GetComponent<Image>().color = Color.gray;
            blueThunder.gameObject.SetActive(true);
        }
        

        if (photonView.IsMine) //자신의 것일 경우
        {
            //1번 관절
            skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
            //2번 몸체
            skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));
            //플레이어 이름 색
            playerName.GetComponent<Text>().color = new Color(0.08f, 0.22f, 0.25f, 0);
        }
        else if (!photonView.IsMine) //자신의 것이 아닐 경우
        {
            //1번 관절
            skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.6117647f, 0.3882353f, 0.3490196f, 1));
            //2번 몸체
            skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.9254902f, 0.5843138f, 0.5490196f, 1));
            //플레이어 이름 색
            playerName.GetComponent<Text>().color = new Color(0.25f, 0.22f, 0.08f, 0);
        }
        transform.parent = gameManager.playerGroup.transform;
    }

    private void LateUpdate()
    {
        //UI 위치 초기화
        if (!isShot)
        {
            playerName.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.forward + Vector3.left * 0.5f);//transform.GetChild(1).
            if (photonView.IsMine) 
            {
            darkThunder.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.forward + Vector3.left * 2.35f);
            blueThunder.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.forward + Vector3.left * 2.35f);
            }
        }
        else 
        {
            isShot = false;
        }

        if (photonView.IsMine) 
        {
            curTime += Time.deltaTime;
            blueThunder.GetComponent<Image>().fillAmount = curTime / maxTime;
        }
    }

    private void OnEnable() => Revive();
   
    public void Revive()
    {
        //오브젝트 활성화
        gameObject.SetActive(true);
        //위치 초기화
        transform.position = gameManager.playerGroup.transform.position + new Vector3(0,0,Random.Range(-4, 2));
        //죽었을 때 충돌하지 않도록
        col.enabled = true;
        //애니메이션
        anim.SetBool("isLive", true);
        anim.SetBool("isRun", false);
        anim.SetTrigger("isRevive");
        //왜곡장 해제
        VisibleDissolve();
        //시작 영역 활성화
        gameManager.chapterArea.SetActive(true);
        //경로 관리
        if(photonView.IsMine)
            spot.transform.position = transform.position;
        isControl = false;
        agent.enabled = true;//111111111111111111111111
        if(agent.enabled)
        agent.isStopped = true;
        lr.enabled = false;
        //정지중 다시 가려고하면 자동으로 꺼짐 방지
        if (draw != null)
            StopCoroutine(draw);
        //2초부터 활성화
        Invoke("Activate", 2f);
    }

    public void Activate()//2초후 부터 움직이도록
    {
        //사격 시간 측정
        curTime = maxTime;
        //통제 가능
        isControl = true;
        //2초 후부터 플레이어 이름 UI 활성화
        Color nameColor = playerName.GetComponent<Text>().color;
        playerName.GetComponent<Text>().color = new Color(nameColor.r, nameColor.g, nameColor.b, 1);
        //이미지 관리
        if (photonView.IsMine) 
        {
        darkThunder.GetComponent<Image>().color = Color.gray;
        blueThunder.GetComponent<Image>().color = Color.white;
        }
    }

    private void OnTriggerEnter(Collider other)//적이 충돌함
    {
        if (other.gameObject.tag == "EnemyAttack" && !isDissolve && gameManager.EnemiesCount > 0 ) 
        {
            Bullet otherBullet = other.gameObject.GetComponent<Bullet>();

            //데미지 계산은 피격자만 함
            if (photonView.IsMine)
            {
                //데미지 계산
                photonView.RPC("SoonDie", RpcTarget.AllBuffered);
                //총알이면 비활성화
                if (otherBullet.isBullet)
                    otherBullet.photonView.RPC("BulletOff", RpcTarget.AllBuffered);
            }    
        }
        else if (other.gameObject.tag == "StageStart" && photonView.IsMine)// && SceneManager.GetActiveScene().name == "TmpScene"
            gameManager.photonView.RPC("EnterStage", RpcTarget.AllBuffered);//모든 플레이어에게 입장을 알림
    }

    #region 곧죽음
    [PunRPC]
    void SoonDie()//죽는 애니메이션에서 사용 
    {
        //핏자국
        particle.Play();
        //피격 소리
        gameManager.audioManager.PlaySfx(AudioManager.Sfx.Impact, true);
        //통제 불가
        isControl = false;
        //경로 설정
        if (agent.enabled) 
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
            

        lr.enabled = false;
        //정지중 다시 가려고하면 자동으로 꺼짐 방지
        if (draw != null)
            StopCoroutine(draw);
        //죽었을 때 충돌하지 않도록
        col.enabled = false;
        //애니메이션
        anim.SetBool("isRun", false);
        anim.SetBool("isLive", false);
        anim.SetTrigger("isDie");
        //플레이어 이름 관리
        Color nameColor = playerName.GetComponent<Text>().color;
        playerName.GetComponent<Text>().color = new Color(nameColor.r, nameColor.g, nameColor.b, 0);
        //스킬 바 관리
        if (photonView.IsMine) 
        {
            Color thunderColor = new Color(0, 0, 0, 0);
            darkThunder.GetComponent<Image>().color = thunderColor;
            blueThunder.GetComponent<Image>().color = thunderColor;
        }    
    }
    #endregion

    #region 바로 죽음
    void RealDie() //죽는 애니메이션에서 사용
    {     
        CancelInvoke();
        //오브젝트 비활성화
        gameObject.SetActive(false);
    }

    #endregion

    #region 사격제어
    [PunRPC]
    void ShotControl()
    {
        //플레이어 UI 관리를 위한 불값
        isShot = true;
        //소리 재생
        gameManager.audioManager.PlaySfx(AudioManager.Sfx.PlayerBulletA, true);
    }
    #endregion

    #region 쳐다보기
    void targetControl() 
    {
        int layerMask = LayerMask.GetMask("Territory"); // "Territory" 레이어와 충돌하도록 설정
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
            // 트리거는 무시한다
            spot.position = hit.point;
    }
    #endregion

    void Update()
    {
        if (photonView.IsMine && isControl && !gameManager.isChat)//로컬이 아니면 취소
        {
           
            if (Input.GetKeyDown(KeyCode.Q) && curTime >= maxTime)
            {
                #region 플레이어공격A
                //무반동 삭제
                curTime = 0f;
                
                //일단 정지
                agent.isStopped = true;
                lr.enabled = false;
                anim.SetBool("isRun", false);
                anim.SetTrigger("isAttack");
                //위치 고정
                spot.position = transform.position;
                agent.velocity = Vector3.zero;
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //목표 고정
                targetControl();
                
                //사격한 지점을 보도록
                transform.LookAt(spot.position);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                //투사체 생성
                GameObject bullet = gameManager.Get("PlayerBulletA");
                //투사체 잔상 제거
                bullet.GetComponent<Bullet>().trailRenderer.Clear();
                //투사체 위치 조정
                bullet.transform.position = transform.position + new Vector3(0, 1.5f, 0) + transform.forward.normalized;
                //투사체 네트워크를 통한 가속
                bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, transform.forward);
                //투사체 잔상 제거
                bullet.GetComponent<Bullet>().trailRenderer.Clear();
                //UI관리를 위해 모두에게 알리기
                photonView.RPC("ShotControl", RpcTarget.AllBuffered);
                //업적 관리
                gameManager.archiveNoShot = false;//챕터 중, 단 한 발도 발사하지 않음(쏘면 false)(1)
                #endregion
            }
            
            else if (Input.GetMouseButton(1))
            {
                if (Input.GetMouseButtonDown(1)) 
                {
                    gameManager.audioManager.PlaySfx(AudioManager.Sfx.Step, true);
                }
                #region 마우스 이동
                targetControl();
               
                    //다시 움직이기시작함
                    agent.isStopped = false;
                    //목적지 설정
                    agent.SetDestination(spot.position);//hit.point
                    //애니메이션 실행
                    anim.SetBool("isRun", true);
                    //이미 실행중이라면 종료
                    if (draw != null) StopCoroutine(draw);
                    //경로 보이게 라인 렌더러 코루틴실행
                    draw = StartCoroutine(DrawPath());
               
            }
            //도착함
            else if (agent.remainingDistance < 0.15f)
            {
                //애니메이션
                anim.SetBool("isRun", false);
                //라인 렌더러 종료
                lr.enabled = false;
                if (draw != null) //정지중 다시 가려고하면 자동으로 꺼짐 방지
                    StopCoroutine(draw);//시작했던 코루틴 종료-----------------------------------
            }
            #endregion
        }
    }
    #region 이동 경로 보여주기
    IEnumerator DrawPath()
    {
        //yield return null;
        lr.enabled = true;
        while (isControl)
        {
            int cnt = agent.path.corners.Length;//가는 경로를 점으로 표기했을 때, 점의 갯수
            lr.positionCount = cnt;
            for (int i = 0; i < cnt; i++)
            {
                lr.SetPosition(i, agent.path.corners[i]);//점들을 표기
            }
            yield return null;
        } 
    }
    #endregion

    #region 왜곡장
    public void InvisibleDissolve() // 점차 안보이게 되는 것
    {
        StopCoroutine(Dissolve(false));
        StartCoroutine(Dissolve(true));
    }
    public void VisibleDissolve() //점차 보이게 되는 것 
    {
        StopCoroutine(Dissolve(true));
        StartCoroutine(Dissolve(false));
    }
    private IEnumerator Dissolve(bool b)
    {
        if (b) isDissolve = true;
        float firstValue = b ? 0f : 1f;      //true는 InvisibleDissolve(2초)
        float targetValue = b ? 1f : 0f;     //false는 VisibleDissolve(3초)

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;//진행률
            float value = Mathf.Lerp(firstValue, targetValue, progress);

            elapsedTime += Time.deltaTime;

            skinnedMeshRenderer[0].material.SetFloat("_AlphaControl", value);
            skinnedMeshRenderer[1].material.SetFloat("_AlphaControl", value);
            yield return null;
        }
        if (!b) isDissolve = false;
        skinnedMeshRenderer[0].material.SetFloat("_AlphaControl", targetValue);
        skinnedMeshRenderer[1].material.SetFloat("_AlphaControl", targetValue);

    }
    #endregion
}
