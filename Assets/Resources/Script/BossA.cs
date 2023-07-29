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

        //왜곡장을 위한 렌더러 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

        nms = gameManager.GetComponent<NavMeshSurface>();
        agent = GetComponent<NavMeshAgent>();

        audioManager = gameManager.audioManager;
    }

    private void FixedUpdate()
    {
        if (health > 0 && agent.enabled)//추적중이라면 물리법칙 무시    && photonView.IsMine
        {
            if (agent.isStopped)
            {
                //설정 안하면 충돌 시, 끝까지 밀려남
                rigid.velocity = Vector3.zero;
                rigid.angularVelocity = Vector3.zero;
                return;
            }

            if (curActionNum == 0)//삼연격
            {
                //공격 사거리 확인
                float targetRadius = 1f;
                float targetRange = 2.5f;
                //Vector3.forward는 월드 좌표, transform.forward는 로컬 좌표
                RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange,
                LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                if (rayHits.Length > 0)
                {
                        photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 0);
                }
            }
            else if (curActionNum == 1) //검기
            {
                    photonView.RPC("ControlAttack", RpcTarget.AllBuffered, 1);     
            }
            else if (curActionNum == 2) //검기
            {
                //공격 사거리 확인
                float targetRadius = 1f;
                float targetRange = 4.5f;
                //Vector3.forward는 월드 좌표, transform.forward는 로컬 좌표
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
    public void ControlAttack(int index) //공격 활성화
    {
        //소리 재생
        audioManager.PlaySfx(AudioManager.Sfx.BossA, true);
        //공격 경로 활성화
        trail.enabled = true;
        trail.Clear();
        box.enabled = true;

        //애니메이션
        anim.SetBool("isRun", false);
       
        //자동 이동 일시 정지
        if (agent.enabled)
        {
            bool isAnime = anim.GetCurrentAnimatorStateInfo(0).IsName("1") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName("Throw") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName("360 Attack");

            if (!isAnime)//그 어떤 공격 애니메이션도 수행 중이지 않다면
            {
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                    //애니메이션 하나라도 진행중이면 true

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

    #region 다음 동작을 위한 대기
    void StopControl() //애니메이션 후처리
    {
        //후처리
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
        //공격 경로 보이게
        trail.enabled = false;
        box.enabled = false;
        //애니메이션
        anim.SetBool("isRun", true);
        //다음 공격 준비
        if (++curActionNum == 3) curActionNum = 0;
        //자동 이동 활성화
        if (agent.enabled)
            agent.isStopped = false;
    }
    #endregion

    #region 삼연격
    void InstantiateEffect(int EffectNumber)
    {
        if (gameManager.photonView.IsMine)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                //투사체 생성
                GameObject bullet = gameManager.Get("EnemyBulletA");
                //투사체 위치 조정 
                bullet.transform.position = transform.position;// + transform.forward.normalized * 1f
                //투사체 방향 조정
                bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + i * EffectNumber * 36, 0);
                //투사체 네트워크를 통한 가속 조정
                bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                //투사체 방향 재조정
                bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y + i * EffectNumber * 36, 0);
                //파티클
                ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                bulletParticle.Stop();
                bulletParticle.Simulate(2f); // 예시에서는 1초로 설정하여 이미 2초 경과된 상태로 생성
                                             //bulletParticle.Play(); //회전 자체는 매터리얼이 하므로 필요없음(로컬로 처리하니까 안 사라지더라)
                if (EffectNumber == 0 || EffectNumber == 5) break;
            }
        }
    }
    #endregion

    #region 던지기
    void TargetSlash()
    {
        if (photonView.IsMine)
        {
            //투사체 생성
            GameObject bullet = gameManager.Get("EnemyBulletB");
            //투사체 위치 조정 
            bullet.transform.position = targetPos;
            //투사체 네트워크
            bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
            //목표 위치 재설정
            targetPos = target.transform.position;
        }
    }
    #endregion

    #region 회전격
    void Slash360()
    {
        if (gameManager.photonView.IsMine)
        {
            for (int i = 1; i <= 3; ++i)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    GameObject bullet = gameManager.Get("EnemyBulletA");
                    //투사체 위치 조정 
                    bullet.transform.position = transform.position + transform.forward.normalized * i;
                    //투사체 방향 조정
                    bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + j * 90, 0);
                    //투사체 네트워크를 통한 가속 
                    bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                    //투사체 방향 재조정
                    bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y + j * 90, 0);
                    //파티클
                    ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                    bulletParticle.Stop();
                    bulletParticle.Simulate(2f); // 예시에서는 1초로 설정하여 이미 2초 경과된 상태로 생성
                                                 //bulletParticle.Play(); //회전 자체는 매터리얼이 하므로 필요없음(로컬로 처리하니까 안 사라지더라)
                }
            }
        }
    }
    #endregion
  
    private void OnDisable()
    {
        //공격 경로 초기화
        trail.enabled = false;
        box.enabled = false;
        //공격 순서 초기화
        curActionNum = 0;
        //초기화
        CancelInvoke();
    }
}
