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
    [Header("�ڽ� ��ũ��Ʈ���� ����ϱ� ���� ������Ʈ")]
    public NavMeshSurface nms;//ai���   
    public GameManager gameManager;
    public PhotonView photonView;
    public NavMeshAgent agent;//ai
    public Rigidbody rigid;
    public ParticleSystem blood;
    public Animator anim;
    public SkinnedMeshRenderer skinnedMeshRenderer = new SkinnedMeshRenderer();
    public CapsuleCollider col;

    [Header("���� ������Ʈ")]
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

        //�ְ����� ���� ������ 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

        if (isUseNav) //ai�� �̿��ϴ� ��츸
        {
            nms = gameManager.GetComponent<NavMeshSurface>();
            agent = GetComponent<NavMeshAgent>();
        }
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        //�θ� ����
        transform.parent = gameManager.transform;
    }

    private void Start()
    {
        //ai�� �̿��ϴ� ��츸
        if (isUseNav)
            nms.BuildNavMesh();


    }

    private void Update()
    {
        if (!target.activeSelf && photonView.IsMine) //Ÿ���� ���� �� ��� ���� ����
            TargetChange();
            //photonView.RPC("TargetChange", RpcTarget.AllBuffered);

        if (isUseNav && agent.enabled) //ai�� �̿��ϴ� ��츸  && photonView.IsMine
        {
            //Ÿ������ �̵�
            agent.SetDestination(target.transform.position);
        }
    }

    private void LateUpdate()
    {
        //UI ��ġ �ʱ�ȭ(���� ��ġ ��Ҹ� ���ͼ� ����)
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
        //ü�� ȸ��
        health = maxHealth;
        //ü�¹� ����
        Bars = gameManager.Get("Bars");
        Bars.SetActive(true);
        grayBar = Bars.transform.GetChild(0).GetComponent<Image>();
        redBar = Bars.transform.GetChild(1).GetComponent<Image>();
        redBar.fillAmount = 1;

        if (photonView.IsMine) //Ÿ���� ���� �� ��� ���� ����
            TargetChange();


        //�׾��� �� �̵����� �ʵ���
        col.enabled = true;
        //�ְ��� ����
        VisibleDissolve();
        isDissolve = true;
        
        if (isUseNav)
        {
            anim.SetBool("isRun", false);
            //AI
            agent.enabled = false;          
        }       
        //1.5�ʺ��� Ȱ��ȭ
        Invoke("Activate", 1.5f);            
    }
    #region ���� ��, 1.5�ʺ��� ������
    public void Activate()//1.5���� ���� �����̵���
    {
        if (!gameObject.activeSelf) 
            return;
        //����
        isControl = true;
        //�ִϸ��̼�
        anim.SetBool("isLive", true);

        if (isUseNav)//AI�� �̿��Ѵٸ�
        {
            // �ִϸ��̼�
            anim.SetBool("isRun", true);
            agent.enabled = true;
            agent.isStopped = false;
        }    
    }
    #endregion

    private void OnTriggerEnter(Collider other)//���� �浹��
    {
        Hitby(other);
    }

    #region �÷��̾�� ���ݹ��� ��
    public void Hitby(Collider other) //EnemyB, C���� ����Ʈ�� 2�� ���Ƿ� �и�
    {
        if ((other.gameObject.tag == "PlayerAttack" && !isDissolve) || other.gameObject.tag == "AbsoluteAttack") 
        {
            Bullet BulletScript = other.gameObject.GetComponent<Bullet>();

            //��� ó���� �Ѿ� ������ ���常 ��
            if (BulletScript.photonView.IsMine)
            {
                //���忡�� ���� ó��
                bool hitbyPlayer = other.gameObject.tag == "PlayerAttack" ? true : false;
                photonView.RPC("DamageControl", RpcTarget.AllBuffered, hitbyPlayer, BulletScript.dmg);
                if (health <= 0)
                {
                    //���忡�� ��� ó��
                    photonView.RPC("SoonDie", RpcTarget.AllBuffered, hitbyPlayer);
                }
                //�Ѿ� ����
                if (BulletScript.isBullet)
                    BulletScript.photonView.RPC("BulletOff", RpcTarget.AllBuffered);
            }
        }
    }
    #endregion

    [PunRPC]
    public void DamageControl(bool hitbyPlayer, int dmg) 
    {
        //�÷��̾��� ������ ��� ���ڱ�
        if (hitbyPlayer)
            blood.Play();
        //������ ���
        health -= dmg;
        //ü�� �� ���
        redBar.fillAmount = (float)health / (float)maxHealth;
    }

    #region ������
    [PunRPC]
    public void SoonDie(bool hitbyPlayer)//�״� �ִϸ��̼��� ������ ��, ��� (�̺�Ʈ�� ������ ����)
    {
            //����
            isControl = false;
            //ü��
            health = 0;
            //�׾��� �� �浹���� �ʵ���
            col.enabled = false;
            StopCoroutine(Dissolve(false));
            if (isUseNav) //AI ��� ��
            {
                //ai ��Ȱ��ȭ
                agent.enabled = false;
                //�ִϸ��̼�
                anim.SetBool("isRun", false);
            }
            anim.SetBool("isLive", false);
            


            if (hitbyPlayer) //�÷��̾ ���� �¾��� ����, ��� �ִϸ��̼�
                anim.SetTrigger("isDie");
            else //���� ���� ������ ���
            {
                StartCoroutine(Dissolve(true));
                Invoke("RealDiebyAbsolute", 1.5f);
            }
            //ui ����
            Bars.SetActive(false);
    }
    #endregion

    #region �ٷ� ����
    void RealDie() //�״� �ִϸ��̼ǿ��� ���
    {
        CancelInvoke();
        //�� �� ���
        gameManager.EneniesCountControl();

        gameObject.SetActive(false);
    }
    void RealDiebyAbsolute() //���� ������ ����
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }
    #endregion

    #region é�� ���� ��, �̸� ������ �ϱ� ����
    [PunRPC]
    public void RPCfirstInstantaite() 
    {
        gameObject.SetActive(false);
        Bars.SetActive(false);
    }
    #endregion

    #region ������Ʈ Ǯ������ Ȱ��ȭ
    [PunRPC]
    public void RPCActivate(Vector3 vec)//GameObject
    {
        gameObject.SetActive(true);
        transform.position = vec;
    }
    #endregion


    #region ��ǥ ����
    void TargetChange() 
    {
        //������ ����
        int size = gameManager.playerGroup.transform.childCount;
        
        bool isAllDie = true;

        int ran = Random.Range(0, size);
        target = gameManager.playerGroup.transform.GetChild(ran).gameObject;

        if (target.gameObject.activeSelf)//ù ��° �÷��̾ ����ִٸ�
        {
            photonView.RPC("TargetChangeEnd", RpcTarget.AllBuffered, target.GetPhotonView().ViewID);
            isAllDie = false;
        }
        else if(size == 2)//ù ��° �÷��̾ ��������鼭, �÷��̾� ���� 2���� ������
        {
            ran = ran == 0 ? 1 : 0;
            target = gameManager.playerGroup.transform.GetChild(ran).gameObject;
            if (target.gameObject.activeSelf)
            {
                photonView.RPC("TargetChangeEnd", RpcTarget.AllBuffered, target.GetPhotonView().ViewID);
                isAllDie = false;
            }
        }
        
        if (isAllDie)//���� ���� ���
            gameManager.photonView.RPC("AbsoluteReviveStart", RpcTarget.AllBuffered);
        
    }
    [PunRPC]
    public void TargetChangeEnd(int targetViewID)
    {
        target = PhotonView.Find(targetViewID).gameObject;
        // targetObj�� ����Ͽ� ���ϴ� �۾��� �����մϴ�.
    }
    #endregion

    #region �ְ��� 
    public void InvisibleDissolve() // ���� �Ⱥ��̰� �Ǵ� ��
    {
        StopCoroutine(Dissolve(false));
        StartCoroutine(Dissolve(true));
    }
    public void VisibleDissolve() //���� ���̰� �Ǵ� �� 
    {
        if (health > 0) 
        {
            StopCoroutine(Dissolve(true));
            StartCoroutine(Dissolve(false));
        }     
    } 
    IEnumerator Dissolve(bool b)//�ְ��� 1.5�ʰ�
    {
        if (b) isDissolve = true;
        float firstValue = b ? 0f : 1f;      //true�� InvisibleDissolve
        float targetValue = b ? 1f : 0f;     //false�� VisibleDissolve

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
