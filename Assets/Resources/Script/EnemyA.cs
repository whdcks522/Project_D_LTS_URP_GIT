using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class EnemyA : Enemy
{
    public BoxCollider box;
    public TrailRenderer trail;

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
        if (health > 0 && agent.enabled)//�������̶�� ������Ģ ����
        {
            if (agent.isStopped) return;
                //���� ���ϸ� �浹 ��, ������ �з���
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //���� ��Ÿ� Ȯ��
                float targetRadius = 1f;
                float targetRange = 0.75f;
                //Vector3.forward�� ���� ��ǥ, transform.forward�� ���� ��ǥ
                RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange,
                LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
            if (rayHits.Length > 0)
            {
                agent.isStopped = true;
                anim.SetTrigger("isAttack");
            }  
        }
        else if (health <= 0) AttackControl(false);
    }

    void AttackControl(bool b)//���� ó�� ���� 
    {
        //���� ���� ����
        box.enabled = b;
        //������ ����
        trail.enabled = b;
    }

    #region ���� �ִϸ��̼� �̺�Ʈ
    public void AttackBegin() 
    {
        if (health > 0) //���� ���� Ȱ��ȭ
        {
            AttackControl(true);
            trail.Clear();
        }
    }

    public void AttackContinue()
    {
        if (health > 0) //���� ���� ��Ȱ��ȭ
            AttackControl(false); 
    }
    
    public void AttackEnd()
    {
        if (health > 0) 
        {
            //�׺�Ž� Ȱ��ȭ
            agent.isStopped = false;
        }
    }
    #endregion 
}
