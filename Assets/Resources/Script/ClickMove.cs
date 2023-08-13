using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.Animations.Rigging;

public class ClickMove : MonoBehaviourPunCallbacks
{
    
    Animator anim;
    LineRenderer lr;
    Coroutine draw;
    Rigidbody rigid;
    Ray ray;

    public Transform spot;//�̵��� ���
    GameManager gameManager;//���ӸŴ���
    NavMeshAgent agent;
    NavMeshSurface nms;//ai
    PhotonView photonView;//���� ��
    ParticleSystem particle;//��ƼŬ �ý���
    CapsuleCollider col;
    //���͸��� ��ȯ
    SkinnedMeshRenderer []skinnedMeshRenderer = new SkinnedMeshRenderer[2];

    [Header("UI")]
    public GameObject playerName;//�÷��̾� �̸�
    public GameObject darkThunder;//�÷��̾� �̸�
    public GameObject blueThunder;//�÷��̾� �̸�
    bool isControl;
    bool isDissolve;
    bool isShot;
    float curTime = 1f;
    float maxTime = 1f;//��� �� ���ð�

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

        //�׺�Ž�
        nms = gameManager.GetComponent<NavMeshSurface>();
        //���� ������
        lr = GetComponent<LineRenderer>();
        lr.startWidth = 0.8f;
        lr.endWidth = 0.03f;
        //lr.material.color = new Color(0.3f, 0.7f, 0.3f);
        lr.startColor = Color.green; //�� ������ ��
        lr.endColor = Color.red; //�� ���� ��

        //AI ���
        nms.BuildNavMesh();

        //���͸��� ����
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "BookScene") 
        {
            //1�� ����
            skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
            //2�� ��ü
            skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));
            return;
        }
            

            playerName.GetComponent<Text>().text = photonView.IsMine? PhotonNetwork.NickName : photonView.Owner.NickName;
        
        if (photonView.IsMine) //�ڽ��� ���� ���
        {
            //��ų ��
            darkThunder.gameObject.SetActive(true);
            darkThunder.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0);

            blueThunder.gameObject.SetActive(true);
            blueThunder.GetComponent<Image>().color = new Color(1, 1, 1, 0);

            //1�� ����
            skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.427451f, 0.4980391f, 0.5098039f, 1));
            //2�� ��ü
            skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.345098f, 0.682353f, 0.7490196f, 1));
            //�÷��̾� �̸� ��
            playerName.GetComponent<Text>().color = new Color(0.596f, 0.910f, 0.980f, 0);//new Color(0.08f, 0.22f, 0.25f, 0);
        }
        else if (!photonView.IsMine) //�ڽ��� ���� �ƴ� ���
        {
            //1�� ����
            skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.6117647f, 0.3882353f, 0.3490196f, 1));
            //2�� ��ü
            skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.9254902f, 0.5843138f, 0.5490196f, 1));
            //�÷��̾� �̸� ��
            playerName.GetComponent<Text>().color = new Color(0.98f, 0.596f, 0.643f, 0);//new Color(0.25f, 0.22f, 0.08f, 0);
        }
        transform.parent = gameManager.playerGroup.transform;
    }

    private void LateUpdate()
    {
        if (SceneManager.GetActiveScene().name == "BookScene") 
            return;

        //UI ��ġ �ʱ�ȭ
        if (!isShot)
        {
            playerName.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.forward + Vector3.left * 0.5f);//transform.GetChild(1).
            if (photonView.IsMine) 
            {
            darkThunder.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.forward + Vector3.right * 1.2f);
            blueThunder.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.forward + Vector3.right * 1.2f);
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

    private void OnEnable() 
    {
        if (SceneManager.GetActiveScene().name != "BookScene")
            Revive();
    } 
   
    public void Revive()
    {
        //������Ʈ Ȱ��ȭ
        gameObject.SetActive(true);
        //��ġ �ʱ�ȭ
        if(gameManager.photonView.IsMine)
            transform.position = gameManager.playerGroup.transform.position + new Vector3(0, 0, 2);
        else
            transform.position = gameManager.playerGroup.transform.position + new Vector3(0, 0, -2);
        //�׾��� �� �浹���� �ʵ���
        col.enabled = true;
        //�ִϸ��̼�
        anim.SetBool("isLive", true);
        anim.SetBool("isRun", false);
        anim.SetTrigger("isRevive");
        //�ְ��� ����
        VisibleDissolve();
        //���� ���� Ȱ��ȭ
        gameManager.chapterArea.SetActive(true);
        //��� ����
        if(photonView.IsMine)
            spot.transform.position = transform.position;

        isControl = false;
        agent.enabled = true;
        if(agent.enabled)
        agent.isStopped = true;
        lr.enabled = false;
        //������ �ٽ� �������ϸ� �ڵ����� ���� ����
        if (draw != null)
            StopCoroutine(draw);
        //2�ʺ��� Ȱ��ȭ
        Invoke("Activate", 2f);
    }

    public void Activate()//2���� ���� �����̵���
    {
        //��� �ð� ����
        curTime = maxTime;
        //���� ����
        isControl = true;
        //2�� �ĺ��� �÷��̾� �̸� UI Ȱ��ȭ
        Color nameColor = playerName.GetComponent<Text>().color;
        playerName.GetComponent<Text>().color = new Color(nameColor.r, nameColor.g, nameColor.b, 1);
        //�̹��� ����
        if (photonView.IsMine) 
        {
        darkThunder.GetComponent<Image>().color = Color.gray;
        blueThunder.GetComponent<Image>().color = Color.white;
        }
    }

    private void OnTriggerEnter(Collider other)//���� �浹��
    {
        if (other.gameObject.tag == "EnemyAttack" && !isDissolve && gameManager.EnemiesCount > 0 ) 
        {
            Bullet otherBullet = other.gameObject.GetComponent<Bullet>();

            //������ ����� �ǰ��ڸ� ��
            if (photonView.IsMine)
            {
                //������ ���
                photonView.RPC("SoonDie", RpcTarget.AllBuffered);
                //�Ѿ��̸� ��Ȱ��ȭ
                if (otherBullet.isBullet)
                    otherBullet.photonView.RPC("BulletOff", RpcTarget.AllBuffered);
            }    
        }
        else if (other.gameObject.tag == "StageStart" && photonView.IsMine)
            gameManager.photonView.RPC("EnterStage", RpcTarget.AllBuffered);//��� �÷��̾�� ������ �˸�
    }

    #region ������
    [PunRPC]
    void SoonDie()//�״� �ִϸ��̼ǿ��� ��� (�� �� �����)
    {
        //���ڱ�
        particle.Play();
        //�ǰ� �Ҹ�
        gameManager.audioManager.PlaySfx(AudioManager.Sfx.Impact, true);
        //���� �Ұ�
        isControl = false;

        //��� ����
        if (agent.enabled) 
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        lr.enabled = false;
        //������ �ٽ� �������ϸ� �ڵ����� ���� ����
        if (draw != null)
            StopCoroutine(draw);

        //�׾��� �� �浹���� �ʵ���
        col.enabled = false;

        //�ִϸ��̼�
        anim.SetBool("isRun", false);
        anim.SetBool("isLive", false);
        anim.SetTrigger("isDie");
        //�÷��̾� �̸� ����
        Color nameColor = playerName.GetComponent<Text>().color;
        playerName.GetComponent<Text>().color = new Color(nameColor.r, nameColor.g, nameColor.b, 0);
        //��ų �� ����
        if (photonView.IsMine) 
        {
            Color thunderColor = new Color(0, 0, 0, 0);
            darkThunder.GetComponent<Image>().color = thunderColor;
            blueThunder.GetComponent<Image>().color = thunderColor;
        }    
    }
    #endregion

    #region �ٷ� ����
    void RealDie() //�״� �ִϸ��̼ǿ��� ���
    {     
        CancelInvoke();
        //������Ʈ ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    #endregion

    #region �������
    [PunRPC]
    void ShotControl()
    {
        //�÷��̾� UI ������ ���� �Ұ�
        isShot = true;
        //�Ҹ� ���
        gameManager.audioManager.PlaySfx(AudioManager.Sfx.PlayerBulletA, true);
    }
    #endregion

    #region �Ĵٺ���
    void targetControl() 
    {
        int layerMask = LayerMask.GetMask("Territory"); // "Territory" ���̾�� �浹�ϵ��� ����
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
            // Ʈ���Ŵ� �����Ѵ�
            spot.position = hit.point;
    }
    #endregion

    void Update()
    {
        if (photonView.IsMine && isControl && !gameManager.isChat)//������ �ƴϸ� ���
        {

            if (Input.GetKeyDown(KeyCode.Q) && curTime >= maxTime)
            {
                #region �÷��̾����A
                //���ݵ� ����
                curTime = 0f;

                //�ϴ� ����
                agent.isStopped = true;
                lr.enabled = false;
                anim.SetBool("isRun", false);
                anim.SetTrigger("isAttack");
                //��ġ ����
                spot.position = transform.position;
                agent.velocity = Vector3.zero;
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //��ǥ ����
                targetControl();

                //����� ������ ������
                transform.LookAt(spot.position);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                //����ü ����
                GameObject bullet = gameManager.Get("PlayerBulletA");
                //����ü ��ġ ����
                bullet.transform.position = transform.position + new Vector3(0, 1.5f, 0) + transform.forward.normalized;
                //����ü ��Ʈ��ũ�� ���� ����
                bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, transform.forward);
                //����ü �ܻ� ����
                bullet.GetComponent<Bullet>().photonView.RPC("TrailClear", RpcTarget.AllBuffered);
                //UI������ ���� ��ο��� �˸���
                photonView.RPC("ShotControl", RpcTarget.AllBuffered);
                //���� ����
                gameManager.archiveNoShot = false;//é�� ��, �� �� �ߵ� �߻����� ����(��� false)(1)
                #endregion
            }

            else if (Input.GetMouseButton(1))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    
                    gameManager.audioManager.PlaySfx(AudioManager.Sfx.Step, true);
                }
                #region ���콺 �̵�
                targetControl();

                //�ٽ� �����̱������
                agent.isStopped = false;
                //������ ����
                agent.SetDestination(spot.position);//hit.point
                                                    //�ִϸ��̼� ����
                anim.SetBool("isRun", true);
                //�̹� �������̶�� ����
                if (draw != null) StopCoroutine(draw);
                //��� ���̰� ���� ������ �ڷ�ƾ����
                draw = StartCoroutine(DrawPath());
            }
            //������
            else if (agent.remainingDistance < 0.15f)
            {
                //�ִϸ��̼�
                anim.SetBool("isRun", false);
                //���� ������ ����
                lr.enabled = false;
                if (draw != null) //������ �ٽ� �������ϸ� �ڵ����� ���� ����
                    StopCoroutine(draw);//�����ߴ� �ڷ�ƾ ����-----------------------------------
            }
            #endregion
        }
        else if (SceneManager.GetActiveScene().name == "BookScene")
        {
            #region å ������ �̵�
            if (Input.GetMouseButton(1))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    gameManager.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
                }
                targetControl();

                //�ٽ� �����̱������
                agent.isStopped = false;
                //������ ����
                agent.SetDestination(spot.position);//hit.point
                                                    //�ִϸ��̼� ����
                anim.SetBool("isRun", true);
            }
            //������ 
            else if (agent.remainingDistance < 0.15f)
            {
                anim.SetBool("isRun", false);
            }
            #endregion
        }
    }
    #region �̵� ��� �����ֱ�
    IEnumerator DrawPath()
    {
        bool isFirst = true;
        while (isControl)
        {
            int cnt = agent.path.corners.Length;//���� ��θ� ������ ǥ������ ��, ���� ����
            lr.positionCount = cnt;
            for (int i = 0; i < cnt; i++)
            {
                lr.SetPosition(i, agent.path.corners[i]);//������ ǥ��
            }
            yield return null;
            if (!isControl) //false�϶� ������(�׾��� ��, �̵� ������ ������ ��� ��� ���̴� ���� �ذ�)
            {
                lr.enabled = false;
                break;
            }
            else if (isFirst)//ù ���� ���Ŀ��� Ȱ��ȭ�� �����ߵ�
            {
                lr.enabled = true;
                isFirst = false;
            }
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
        StopCoroutine(Dissolve(true));
        StartCoroutine(Dissolve(false));
    }
    private IEnumerator Dissolve(bool b)
    {
        if (b) isDissolve = true;
        float firstValue = b ? 0f : 1f;      //true�� InvisibleDissolve(2��)
        float targetValue = b ? 1f : 0f;     //false�� VisibleDissolve(3��)

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;//�����
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
