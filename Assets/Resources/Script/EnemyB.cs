using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class EnemyB : Enemy
{
    public ParticleSystem chargeBall;
    public bool isAttack;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        gameManager = GameManager.Instance;
        photonView = GetComponent<PhotonView>();

        //왜곡장을 위한 렌더러 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();

        //부모 설정
        transform.parent = gameManager.transform;
    }

    private void FixedUpdate()
    {
        if (health > 0 && anim.GetBool("isLive"))//생존 중이면서, 공격중이지 않으면//health > 0 && Bars != null && Bars.activeSelf
        {
            transform.LookAt(target.transform.position);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            if (!isAttack && health > 0) 
            {
                isAttack = true;
                anim.SetTrigger("isAttack");
            }
        }
    }

    private void OnTriggerEnter(Collider other)//적이 충돌함
    {
        Hitby(other);

        if (other.gameObject.tag == "PlayerAttack" && !isDissolve)
            photonView.RPC("RPCEffect", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPCEffect()
    {
        chargeBall.Stop();
    }

    private void OnDisable()
    {
        CancelInvoke();
        isAttack = false;
    }


    #region 공격 애니메이션 이벤트
    void AttackCharge() 
    {
        if (health > 0) 
        {
            chargeBall.Play();
        }
        else isAttack = false;
    }

    public void AttackThrow() 
    {
        if (health > 0 )
        {
            if (gameManager.photonView.IsMine)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    GameObject bullet = gameManager.Get("EnemyBulletA");
                    //투사체 위치 조정 
                    bullet.transform.position = transform.position + Vector3.up + transform.forward.normalized * 1f;
                    //투사체 방향 조정
                    bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + i * 20, 0);
                    //투사체 네트워크를 통한 가속 조정
                    bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                    //투사체 방향 재조정
                    bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y + i * 20, 0);

                    //파티클
                    ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                    bulletParticle.Stop();
                    bulletParticle.Simulate(2f); // 예시에서는 1초로 설정하여 이미 2초 경과된 상태로 생성
                                                 //bulletParticle.Play(); //회전 자체는 매터리얼이 하므로 필요없음(로컬로 처리하니까 안 사라지더라)
                }
            }
        }
        else isAttack = false;
    }


    void AttackEnd() => isAttack = false;


    #endregion
}
