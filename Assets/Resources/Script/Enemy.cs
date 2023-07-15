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
    }

    private void Start()
    {
        //ai를 이용하는 경우만
        if (isUseNav) 
            nms.BuildNavMesh();
    }

    private void Update()
    {
        if (!target.activeSelf) //타켓이 없을 질 경우 새로 정함
            photonView.RPC("TargetChange", RpcTarget.AllBuffered);

        if (isUseNav && agent.enabled) //ai를 이용하는 경우만
        {
            //타겟으로 이동
            agent.SetDestination(target.transform.position);
        }
    }

    private void LateUpdate()
    {
        //UI 위치 초기화
        redBar.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(1).transform.position + Vector3.forward);
        grayBar.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(1).transform.position + Vector3.forward);
    }
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
        //타겟 설정
        photonView.RPC("TargetChange", RpcTarget.AllBuffered);
        //죽었을 때 이동하지 않도록
        col.enabled = true;
        //왜곡장 시작
        VisibleDissolve();
        isDissolve = true;
        
        if (isUseNav)
        { 
            anim.SetBool("isRun", false);
            
            //AI
            agent.enabled = true;
            agent.isStopped = true;
        }       
        //1.5초부터 활성화
        Invoke("Activate", 1.5f);            
    }
    #region 생성 후, 1.5초부터 움직임
    public void Activate()//1.5초후 부터 움직이도록
    {
        //체력바 관리
        Color grayColor = grayBar.color;
        Color redColor = redBar.color;
        grayBar.color = new Color(grayColor.r, grayColor.g, grayColor.b , 1);
        redBar.color = new Color(redColor.r, redColor.g, redColor.b, 1);

        //애니메이션
        anim.SetBool("isLive", true);


        if (isUseNav)//AI를 이용한다면
        {
            // 애니메이션
            anim.SetBool("isRun", true);
            agent.isStopped = false;
        }    
    }
    #endregion

    private void OnTriggerEnter(Collider other)//적이 충돌함
    {
       
        if (other.gameObject.tag == "PlayerAttack" && !isDissolve) //플레이어 공격
                                     Hitby(other);
        else if(other.gameObject.tag == "AbsoluteAttack") //절대 공격 처리
                                     Hitby(other);

    }
    #region 플레이어에게 공격받을 시
    public void Hitby(Collider other) //EnemyB에서 이펙트를 2개 쓰므로 분리
    {
        //핏자국
        if (other.gameObject.tag == "PlayerAttack") blood.Play();
        Bullet otherBullet = other.gameObject.GetComponent<Bullet>();
        //데미지 계산
        health -= otherBullet.dmg;
        redBar.fillAmount = (float)health / (float)maxHealth;

        //사망 처리와 총알 관리는 방장만 함
        if (!gameManager.photonView.IsMine) return;

        if (health <= 0)
            photonView.RPC("SoonDie", RpcTarget.AllBuffered, other.gameObject.tag == "PlayerAttack"? true : false);
        //총알이면 비활성화
        if (otherBullet.isBullet)
            otherBullet.photonView.RPC("BulletOff", RpcTarget.AllBuffered);
    }
    #endregion

    #region 곧죽음
    [PunRPC]
    public void SoonDie(bool hitbyPlayer)//죽는 애니메이션에서 사용 (이벤트로 넣지는 않음)
    {
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
        gameManager.EneniesCountControl();
        gameObject.SetActive(false);
    }
    void RealDiebyAbsolute() //절대 영역에 죽음
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }
    #endregion

    #region 오브젝트 풀링
    [PunRPC]
    public GameObject Allreturn()
    {
        gameObject.SetActive(true);
        return this.gameObject;
    }
    #endregion

    #region 목표 설정
    [PunRPC]
    public void TargetChange() 
    {
        int size = gameManager.playerGroup.transform.childCount;
        if (size == 1) 
        {
            target = gameManager.playerGroup.transform.GetChild(0).gameObject;
            if (!target.activeSelf) 
            {
                StartCoroutine(gameManager.AbsoluteRevive(1));
                
            }
                    
        }
        else if (size == 2)
        {
            target = gameManager.playerGroup.transform.GetChild(0).gameObject;
            if(!target.activeSelf)
                target = gameManager.playerGroup.transform.GetChild(1).gameObject;
            else gameManager.AbsoluteRevive(2);
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
