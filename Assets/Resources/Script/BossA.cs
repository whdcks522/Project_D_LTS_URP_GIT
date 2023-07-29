using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

    public class BossA : Enemy
{
    int curActionNum;
    public TrailRenderer trail;
    public BoxCollider box;
    Vector3 targetPos;
    AudioManager audioManager;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        blood = GetComponent<ParticleSystem>();
        gameManager = GameManager.Instance;

        photonView = GetComponent<PhotonView>();

        //�ְ����� ���� ������ 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

        nms = gameManager.GetComponent<NavMeshSurface>();
        agent = GetComponent<NavMeshAgent>();

        audioManager = gameManager.audioManager;
    }

    private void FixedUpdate()
    {
        if (health > 0 && agent.enabled)//�������̶�� ������Ģ ����    && photonView.IsMine
        {
            if (agent.isStopped)
            {
                //���� ���ϸ� �浹 ��, ������ �з���
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                return;
            }

            if (curActionNum == 0)//�￬��
            {
                //���� ��Ÿ� Ȯ��
                float targetRadius = 1f;
                float targetRange = 2.5f;
                //Vector3.forward�� ���� ��ǥ, transform.forward�� ���� ��ǥ
                RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange,
                LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                if (rayHits.Length > 0)
                {
                        photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 0);
                }
            }
            else if (curActionNum == 1) //�˱�
            {
                    photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 1);     
            }
            else if (curActionNum == 2) //�˱�
            {
                //���� ��Ÿ� Ȯ��
                float targetRadius = 1f;
                float targetRange = 4.5f;
                //Vector3.forward�� ���� ��ǥ, transform.forward�� ���� ��ǥ
                RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange,
                LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                if (rayHits.Length > 0)
                {
                        photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 2); 
                } 
            }
        }
    }

    [PunRPC]
    public void ControlAttack(int index) //���� Ȱ��ȭ
    {
        //�Ҹ� ���
        audioManager.PlaySfx(AudioManager.Sfx.BossA, true);
        //���� ��� Ȱ��ȭ
        trail.enabled = true;
        trail.Clear();
        box.enabled = true;

        //�ִϸ��̼�
        anim.SetBool("isRun", false);
       
        //�ڵ� �̵� �Ͻ� ����
        if (agent.enabled)
        {
            bool isAnime = anim.GetCurrentAnimatorStateInfo(0).IsName("1") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName("Throw") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName("360 Attack");

            if (!isAnime)//�� � ���� �ִϸ��̼ǵ� ���� ������ �ʴٸ�
            {
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                    //�ִϸ��̼� �ϳ��� �������̸� true

                    if (index == 0) anim.SetTrigger("isThree");
                    else if (index == 1)
                    {
                        anim.SetTrigger("isThrow");
                        targetPos = target.transform.position;
                    }
                    else if (index == 2) anim.SetTrigger("isSpin");
                }
            }
        }
        
    }

    #region ���� ������ ���� ���
    void StopControl() //�ִϸ��̼� ��ó��
    {
        //��ó��
        Invoke("StopControlEnd", 0.5f);
    }

    void StopControlContinue()
    {
        if(gameManager.photonView.IsMine)
            photonView.RPC("StopControlEnd", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void StopControlEnd()
    {
        //���� ��� ���̰�
        trail.enabled = false;
        box.enabled = false;
        //�ִϸ��̼�
        anim.SetBool("isRun", true);
        //���� ���� �غ�
        if (++curActionNum == 3) curActionNum = 0;
        //�ڵ� �̵� Ȱ��ȭ
        if (agent.enabled)
            agent.isStopped = false;
    }
    #endregion

    #region �￬��
    void InstantiateEffect(int EffectNumber)
    {
        if (gameManager.photonView.IsMine)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                //����ü ����
                GameObject bullet = gameManager.Get("EnemyBulletA");
                //����ü ��ġ ���� 
                bullet.transform.position = transform.position;// + transform.forward.normalized * 1f
                //����ü ���� ����
                bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + i * EffectNumber * 36, 0);
                //����ü ��Ʈ��ũ�� ���� ���� ����
                bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                //����ü ���� ������
                bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y + i * EffectNumber * 36, 0);
                //��ƼŬ
                ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                bulletParticle.Stop();
                bulletParticle.Simulate(2f); // ���ÿ����� 1�ʷ� �����Ͽ� �̹� 2�� ����� ���·� ����
                                             //bulletParticle.Play(); //ȸ�� ��ü�� ���͸����� �ϹǷ� �ʿ����(���÷� ó���ϴϱ� �� ���������)
                if (EffectNumber == 0 || EffectNumber == 5) break;
            }
        }
    }
    #endregion

    #region ������
    void TargetSlash()
    {
        if (photonView.IsMine)
        {
            //����ü ����
            GameObject bullet = gameManager.Get("EnemyBulletB");
            //����ü ��ġ ���� 
            bullet.transform.position = targetPos;
            //����ü ��Ʈ��ũ
            bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
            //��ǥ ��ġ �缳��
            targetPos = target.transform.position;
        }
    }
    #endregion

    #region ȸ����
    void Slash360()
    {
        if (gameManager.photonView.IsMine)
        {
            for (int i = 1; i <= 3; ++i)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    GameObject bullet = gameManager.Get("EnemyBulletA");
                    //����ü ��ġ ���� 
                    bullet.transform.position = transform.position + transform.forward.normalized * i;
                    //����ü ���� ����
                    bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + j * 90, 0);
                    //����ü ��Ʈ��ũ�� ���� ���� 
                    bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                    //����ü ���� ������
                    bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y + j * 90, 0);
                    //��ƼŬ
                    ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                    bulletParticle.Stop();
                    bulletParticle.Simulate(2f); // ���ÿ����� 1�ʷ� �����Ͽ� �̹� 2�� ����� ���·� ����
                                                 //bulletParticle.Play(); //ȸ�� ��ü�� ���͸����� �ϹǷ� �ʿ����(���÷� ó���ϴϱ� �� ���������)
                }
            }
        }
    }
    #endregion
  
    private void OnDisable()
    {
        //���� ��� �ʱ�ȭ
        trail.enabled = false;
        box.enabled = false;
        //���� ���� �ʱ�ȭ
        curActionNum = 0;
        //�ʱ�ȭ
        CancelInvoke();
    }
}
