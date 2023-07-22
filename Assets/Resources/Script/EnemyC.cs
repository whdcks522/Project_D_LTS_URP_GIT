using System.Collections;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Photon.Pun.Demo.Asteroids;

public class EnemyC : Enemy
{
    public ParticleSystem earthQuake;
    public CapsuleCollider earthCol;
    
    public Vector3 attackPos;
    public bool isAttack;
    public bool isAttack2;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        gameManager = GameManager.Instance;
        photonView = GetComponent<PhotonView>();

        //왜곡장을 위한 렌더러 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
    }
    private void FixedUpdate()
    {
        if (health > 0 && anim.GetBool("isLive") && !isAttack)//생존 중이면서, 공격중이지 않으면//Bars != null && Bars.activeSelf(중간 2개)
        {
                //시야 관리
                transform.LookAt(target.transform.position);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                //공격 처리를 위함
                isAttack = true;
                //시야를 위함
                isAttack2 = true;
                //위치 저장
                attackPos = transform.position + (target.transform.position - transform.position).normalized * 2f;
                //투명화
                InvisibleDissolve();
                //애니메이션 실행
                anim.SetTrigger("isAttack");
        }
        else if (!isAttack2 && health > 0)
        {
            //시야 관리
            transform.LookAt(target.transform.position);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }

    private void OnTriggerEnter(Collider other)//적이 충돌함
    {
            Hitby(other);
        if (other.gameObject.tag == "PlayerAttack" && !isDissolve) //플레이어 공격
            photonView.RPC("RPCEffect", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPCEffect() 
    {
        earthQuake.Stop();
    }



    private void OnDisable() 
    {
        earthCol.enabled = false;
        attackPos = Vector3.zero;
        isAttack = false;
        isAttack2 = false;
        CancelInvoke();
    } 

    #region 공격 애니메이션 이벤트

    public void AttackStart()
    {
        if (health > 0)
        {
            //공격 처리
            earthQuake.Play();
            earthCol.enabled = true;
        }
        else isAttack = false;
    }

    void AttackStay() => //공격 처리
            earthCol.enabled = false;

    public void AttackContinue() 
    {
        if (health > 0)
        {
              
            //실제 위치 이동
            transform.position = attackPos;
            isAttack2 = false;
            //다음 실행을 위한 대기
            CancelInvoke();
            Invoke("AttackEnd", 2f);
        }
        else isAttack = false;
    }

    void AttackEnd() => isAttack = false;
     
    #endregion
}
