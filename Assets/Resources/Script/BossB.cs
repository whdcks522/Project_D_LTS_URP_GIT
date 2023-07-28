using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BossB : Enemy
{
    int curActionNum;
    public TrailRenderer trail;
    public BoxCollider box;
    Vector3 burstVec;
    bool isLook = true;

    private void OnDisable()
    {
        //���� ��� �ʱ�ȭ
        trail.enabled = false;
        box.enabled = false;
        //���� ���� �ʱ�ȭ
        curActionNum = 0;
        isLook = true;
        //�ʱ�ȭ
        CancelInvoke();
    }

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

            if (curActionNum == 0)//����
            {
                //���� ��Ÿ� Ȯ��
                float targetRadius = 1f;
                float targetRange = 15f;
                //Vector3.forward�� ���� ��ǥ, transform.forward�� ���� ��ǥ
                RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange,
                LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                if (rayHits.Length > 0)
                {
                    photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 0);
                }
            }

            else if (curActionNum == 1) //����
            {
                photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 1);
            }

            else if (curActionNum == 2) //�˱�
            {
                //���� ��Ÿ� Ȯ��
                float targetRadius = 1f;
                float targetRange = 12f;
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

                    if (index == 0) 
                        anim.SetTrigger("isSlash");
                    else if (index == 1)
                        anim.SetTrigger("isBurst");
                    else if (index == 2) 
                        anim.SetTrigger("isSpin");
                }
            }
        }

    }

    public void Slash()
    {
        if (gameManager.photonView.IsMine)
        {
            
                //����ü ����
                GameObject bullet = gameManager.Get("EnemyBulletA");
                //����ü ��ġ ���� 
                bullet.transform.position = transform.position;
                //����ü ���� ����
                bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                //����ü ��Ʈ��ũ�� ���� ���� ����
                bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                //����ü ���� ������
                bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, 0);
                //��ƼŬ
                ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                bulletParticle.Stop();
                bulletParticle.Simulate(2f); // ���ÿ����� 1�ʷ� �����Ͽ� �̹� 2�� ����� ���·� ����
                                             //bulletParticle.Play(); //ȸ�� ��ü�� ���͸����� �ϹǷ� �ʿ����(���÷� ó���ϴϱ� �� ���������)
        }
    }

    public void Burst(int value)
    {
        if (photonView.IsMine)
        {
            for (int x = -1; x <= 1; x++) 
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    //����ü ����
                    GameObject bullet = gameManager.Get("EnemyBulletB");
                    //���� ����
                    burstVec = new Vector3(x, 0, y).normalized * value;
                    //����ü ��ġ ���� 
                    bullet.transform.position = transform.position + burstVec;
                    //����ü ��Ʈ��ũ
                    bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
                }
            } 
        }
    }
}
