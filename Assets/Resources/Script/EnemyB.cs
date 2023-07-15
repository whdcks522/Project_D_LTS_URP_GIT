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
        #region 플레이어의 공격
        if (other.gameObject.tag == "PlayerAttack" && !isDissolve) //플레이어 공격
        {
            Hitby(other);//파티클 Play하면 오브젝트 내부 모든 파티클이 실행됨
            chargeBall.Stop();
        }
        #endregion
        else if (other.gameObject.tag == "AbsoluteAttack") Hitby(other);
    }

    private void OnDisable()=>isAttack = false;

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
        if (health > 0)
        {
            GameObject bullet = gameManager.Get("EnemyBulletA");
            bullet.transform.position = transform.position + new Vector3(0, 1.5f, 0);
            bullet.GetComponent<Rigidbody>().velocity = transform.forward * 5;
        }
        else isAttack = false;
    }

    void AttackEnd() =>isAttack = false;
    
    #endregion
}
