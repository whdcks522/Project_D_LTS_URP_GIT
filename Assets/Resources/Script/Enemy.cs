using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.VisualScripting;

public class Enemy : MonoBehaviourPunCallbacks
{
    [Header("자식 스크립트에서 사용하기 위한 컴포넌트")]
    public NavMeshSurface nms;//ai경로   
    public GameManager gameManager;
    public PhotonView photonView;
    public NavMeshAgent agent;//ai
    public Rigidbody rigid;
    public ParticleSystem blood;
    public Animator anim;
    public SkinnedMeshRenderer skinnedMeshRenderer = new SkinnedMeshRenderer();
    public CapsuleCollider col;

    [Header("기존 컴포넌트")]
    public GameObject target;
    public int maxHealth;
    public int health;

    public GameObject Bars;
    public Image redBar;
    public Image grayBar;
    
    public bool isUseNav;
    public bool isDissolve;

    public bool isControl;
    private void Awake()
    {
        blood = GetComponent<ParticleSystem>();

        gameManager = GameManager.Instance;
        photonView = GetComponent<PhotonView>();

        //왜곡장을 위한 렌더러 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

        if (isUseNav) //ai를 이용하는 경우만
        {
            nms = gameManager.GetComponent<NavMeshSurface>();
            agent = GetComponent<NavMeshAgent>();
        }
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        //부모 설정
        transform.parent = gameManager.transform;
    }

    private void Start()
    {
        //ai를 이용하는 경우만
        if (isUseNav)
            nms.BuildNavMesh();


    }

    private void Update()
    {
        if (!target.activeSelf && photonView.IsMine) //타켓이 없을 질 경우 새로 정함
            TargetChange();
            //photonView.RPC("TargetChange", RpcTarget.AllBuffered);

        if (isUseNav && agent.enabled) //ai를 이용하는 경우만  && photonView.IsMine
        {
            //타겟으로 이동
            agent.SetDestination(target.transform.position);
        }
    }

    private void LateUpdate()
    {
        //UI 위치 초기화(적의 위치 요소를 빼와서 적용)
        redBar.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(1).transform.position + Vector3.forward);
        grayBar.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(1).transform.position + Vector3.forward);
    }

    /*
    [PunRPC]
    public void OriginControlStart(Vector3 vec) 
    {
        StartCoroutine(OriginConrol(vec));
    }

    public IEnumerator OriginConrol(Vector3 vec)
    {
        float curTime = 0;
        while (curTime < 0.5f)
        {
            agent.Warp(vec);
            curTime += Time.deltaTime;
            yield return null;
        }
    }
    */

    private void OnEnable()
    {
        //체력 회복
        health = maxHealth;
        //체력바 관리
        Bars = gameManager.Get("Bars");
        Bars.SetActive(true);
        grayBar = Bars.transform.GetChild(0).GetComponent<Image>();
        redBar = Bars.transform.GetChild(1).GetComponent<Image>();
        redBar.fillAmount = 1;

        if (photonView.IsMine) //타켓이 없을 질 경우 새로 정함
            TargetChange();


        //죽었을 때 이동하지 않도록
        col.enabled = true;
        //왜곡장 시작
        VisibleDissolve();
        isDissolve = true;
        
        if (isUseNav)
        {
            anim.SetBool("isRun", false);
            //AI
            agent.enabled = false;          
        }       
        //1.5초부터 활성화
        Invoke("Activate", 1.5f);            
    }
    #region 생성 후, 1.5초부터 움직임
    public void Activate()//1.5초후 부터 움직이도록
    {
        if (!gameObject.activeSelf) 
            return;
        //제어
        isControl = true;
        //애니메이션
        anim.SetBool("isLive", true);

        if (isUseNav)//AI를 이용한다면
        {
            // 애니메이션
            anim.SetBool("isRun", true);
            agent.enabled = true;
            agent.isStopped = false;
        }    
    }
    #endregion

    private void OnTriggerEnter(Collider other)//적이 충돌함
    {
        Hitby(other);
    }

    #region 플레이어에게 공격받을 시
    public void Hitby(Collider other) //EnemyB, C에서 이펙트를 2개 쓰므로 분리
    {
        if ((other.gameObject.tag == "PlayerAttack" && !isDissolve) || other.gameObject.tag == "AbsoluteAttack") 
        {
            Bullet BulletScript = other.gameObject.GetComponent<Bullet>();

            //사망 처리와 총알 관리는 방장만 함
            if (BulletScript.photonView.IsMine)
            {
                //방장에서 피해 처리
                bool hitbyPlayer = other.gameObject.tag == "PlayerAttack" ? true : false;
                photonView.RPC("DamageControl", RpcTarget.AllBuffered, hitbyPlayer, BulletScript.dmg);
                if (health <= 0)
                {
                    //방장에서 사망 처리
                    photonView.RPC("SoonDie", RpcTarget.AllBuffered, hitbyPlayer);
                }
                //총알 삭제
                if (BulletScript.isBullet)
                    BulletScript.photonView.RPC("BulletOff", RpcTarget.AllBuffered);
            }
        }
    }
    #endregion

    [PunRPC]
    public void DamageControl(bool hitbyPlayer, int dmg) 
    {
        //플레이어의 공격이 경우 핏자국
        if (hitbyPlayer)
            blood.Play();
        //데미지 계산
        health -= dmg;
        //체력 바 계산
        redBar.fillAmount = (float)health / (float)maxHealth;
    }

    #region 곧죽음
    [PunRPC]
    public void SoonDie(bool hitbyPlayer)//죽는 애니메이션을 실행할 때, 사용 (이벤트로 넣지는 않음)
    {
            //제어
            isControl = false;
            //체력
            health = 0;
            //죽었을 때 충돌하지 않도록
            col.enabled = false;
            StopCoroutine(Dissolve(false));
            if (isUseNav) //AI 사용 시
            {
                //ai 비활성화
                agent.enabled = false;
                //애니메이션
                anim.SetBool("isRun", false);
            }
            anim.SetBool("isLive", false);
            


            if (hitbyPlayer) //플레이어에 의해 맞았을 때만, 사망 애니메이션
                anim.SetTrigger("isDie");
            else //절대 공격 영역일 경우
            {
                StartCoroutine(Dissolve(true));
                Invoke("RealDiebyAbsolute", 1.5f);
            }
            //ui 종료
            Bars.SetActive(false);
    }
    #endregion

    #region 바로 죽음
    void RealDie() //죽는 애니메이션에서 사용
    {
        CancelInvoke();
        //둘 다 계산
        gameManager.EneniesCountControl();

        gameObject.SetActive(false);
    }
    void RealDiebyAbsolute() //절대 영역에 죽음
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }
    #endregion

    #region 챕터 입장 시, 미리 생성을 하기 위함
    [PunRPC]
    public void RPCfirstInstantaite() 
    {
        gameObject.SetActive(false);
        Bars.SetActive(false);
    }
    #endregion

    #region 오브젝트 풀링으로 활성화
    [PunRPC]
    public void RPCActivate(Vector3 vec)//GameObject
    {
        gameObject.SetActive(true);
        transform.position = vec;
    }
    #endregion


    #region 목표 설정
    void TargetChange() 
    {
        //방장이 정함
        int size = gameManager.playerGroup.transform.childCount;
        
        bool isAllDie = true;

        int ran = Random.Range(0, size);
        target = gameManager.playerGroup.transform.GetChild(ran).gameObject;

        if (target.gameObject.activeSelf)//첫 번째 플레이어가 살아있다면
        {
            photonView.RPC("TargetChangeEnd", RpcTarget.AllBuffered, target.GetPhotonView().ViewID);
            isAllDie = false;
        }
        else if(size == 2)//첫 번째 플레이어가 사망했으면서, 플레이어 수가 2명이 넘으면
        {
            ran = ran == 0 ? 1 : 0;
            target = gameManager.playerGroup.transform.GetChild(ran).gameObject;
            if (target.gameObject.activeSelf)
            {
                photonView.RPC("TargetChangeEnd", RpcTarget.AllBuffered, target.GetPhotonView().ViewID);
                isAllDie = false;
            }
        }
        
        if (isAllDie)//전멸 했을 경우
            gameManager.photonView.RPC("AbsoluteReviveStart", RpcTarget.AllBuffered);
        
    }
    [PunRPC]
    public void TargetChangeEnd(int targetViewID)
    {
        target = PhotonView.Find(targetViewID).gameObject;
        // targetObj를 사용하여 원하는 작업을 수행합니다.
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
        if (health > 0) 
        {
            StopCoroutine(Dissolve(true));
            StartCoroutine(Dissolve(false));
        }     
    } 
    IEnumerator Dissolve(bool b)//왜곡장 1.5초간
    {
        if (b) isDissolve = true;
        float firstValue = b ? 0f : 1f;      //true는 InvisibleDissolve
        float targetValue = b ? 1f : 0f;     //false는 VisibleDissolve

        float duration = 1.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (health <= 0 && !b) break;
            float progress = elapsedTime / duration;
            float value = Mathf.Lerp(firstValue, targetValue, progress);
            elapsedTime += Time.deltaTime;

            skinnedMeshRenderer.material.SetFloat("_AlphaControl", value);
            yield return null;
        }
        skinnedMeshRenderer.material.SetFloat("_AlphaControl", targetValue);
        if (!b) isDissolve = false;
    }
    #endregion
}
