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
    }

    private void Start()
    {
        //ai�� �̿��ϴ� ��츸
        if (isUseNav) 
            nms.BuildNavMesh();
    }

    private void Update()
    {
        if (!target.activeSelf) //Ÿ���� ���� �� ��� ���� ����
            photonView.RPC("TargetChange", RpcTarget.AllBuffered);

        if (isUseNav && agent.enabled) //ai�� �̿��ϴ� ��츸
        {
            //Ÿ������ �̵�
            agent.SetDestination(target.transform.position);
        }
    }

    private void LateUpdate()
    {
        //UI ��ġ �ʱ�ȭ
        redBar.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(1).transform.position + Vector3.forward);
        grayBar.transform.position = Camera.main.WorldToScreenPoint(transform.GetChild(1).transform.position + Vector3.forward);
    }
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
        //Ÿ�� ����
        photonView.RPC("TargetChange", RpcTarget.AllBuffered);
        //�׾��� �� �̵����� �ʵ���
        col.enabled = true;
        //�ְ��� ����
        VisibleDissolve();
        isDissolve = true;
        
        if (isUseNav)
        { 
            anim.SetBool("isRun", false);
            
            //AI
            agent.enabled = true;
            agent.isStopped = true;
        }       
        //1.5�ʺ��� Ȱ��ȭ
        Invoke("Activate", 1.5f);            
    }
    #region ���� ��, 1.5�ʺ��� ������
    public void Activate()//1.5���� ���� �����̵���
    {
        //ü�¹� ����
        Color grayColor = grayBar.color;
        Color redColor = redBar.color;
        grayBar.color = new Color(grayColor.r, grayColor.g, grayColor.b , 1);
        redBar.color = new Color(redColor.r, redColor.g, redColor.b, 1);

        //�ִϸ��̼�
        anim.SetBool("isLive", true);


        if (isUseNav)//AI�� �̿��Ѵٸ�
        {
            // �ִϸ��̼�
            anim.SetBool("isRun", true);
            agent.isStopped = false;
        }    
    }
    #endregion

    private void OnTriggerEnter(Collider other)//���� �浹��
    {
       
        if (other.gameObject.tag == "PlayerAttack" && !isDissolve) //�÷��̾� ����
                                     Hitby(other);
        else if(other.gameObject.tag == "AbsoluteAttack") //���� ���� ó��
                                     Hitby(other);

    }
    #region �÷��̾�� ���ݹ��� ��
    public void Hitby(Collider other) //EnemyB���� ����Ʈ�� 2�� ���Ƿ� �и�
    {
        //���ڱ�
        if (other.gameObject.tag == "PlayerAttack") blood.Play();
        Bullet otherBullet = other.gameObject.GetComponent<Bullet>();
        //������ ���
        health -= otherBullet.dmg;
        redBar.fillAmount = (float)health / (float)maxHealth;

        //��� ó���� �Ѿ� ������ ���常 ��
        if (!gameManager.photonView.IsMine) return;

        if (health <= 0)
            photonView.RPC("SoonDie", RpcTarget.AllBuffered, other.gameObject.tag == "PlayerAttack"? true : false);
        //�Ѿ��̸� ��Ȱ��ȭ
        if (otherBullet.isBullet)
            otherBullet.photonView.RPC("BulletOff", RpcTarget.AllBuffered);
    }
    #endregion

    #region ������
    [PunRPC]
    public void SoonDie(bool hitbyPlayer)//�״� �ִϸ��̼ǿ��� ��� (�̺�Ʈ�� ������ ����)
    {
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
        gameManager.EneniesCountControl();
        gameObject.SetActive(false);
    }
    void RealDiebyAbsolute() //���� ������ ����
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }
    #endregion

    #region ������Ʈ Ǯ��
    [PunRPC]
    public GameObject Allreturn()
    {
        gameObject.SetActive(true);
        return this.gameObject;
    }
    #endregion

    #region ��ǥ ����
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
