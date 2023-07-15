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

        //왜곡장을 위한 렌더러 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

        nms = gameManager.GetComponent<NavMeshSurface>();
        agent = GetComponent<NavMeshAgent>();
        
    }

    private void FixedUpdate()
    { 
        if (health > 0 && agent.enabled)//추적중이라면 물리법칙 무시
        {
            if (agent.isStopped) return;
                //설정 안하면 충돌 시, 끝까지 밀려남
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                //공격 사거리 확인
                float targetRadius = 1f;
                float targetRange = 0.75f;
                //Vector3.forward는 월드 좌표, transform.forward는 로컬 좌표
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

    void AttackControl(bool b)//공격 처리 관리 
    {
        //공격 범위 관리
        box.enabled = b;
        //렌더러 관리
        trail.enabled = b;
    }

    #region 공격 애니메이션 이벤트
    public void AttackBegin() 
    {
        if (health > 0) //공격 범위 활성화
        {
            AttackControl(true);
            trail.Clear();
        }
    }

    public void AttackContinue()
    {
        if (health > 0) //공격 범위 비활성화
            AttackControl(false); 
    }
    
    public void AttackEnd()
    {
        if (health > 0) 
        {
            //네비매쉬 활성화
            agent.isStopped = false;
        }
    }
    #endregion 
}
