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

        //�ְ����� ���� ������ 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();    
    }

    private void FixedUpdate()
    {
        if (health > 0 && anim.GetBool("isLive"))//���� ���̸鼭, ���������� ������//health > 0 && Bars != null && Bars.activeSelf
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

    private void OnTriggerEnter(Collider other)//���� �浹��
    {
        #region �÷��̾��� ����
        if (other.gameObject.tag == "PlayerAttack" && !isDissolve) //�÷��̾� ����
        {
            Hitby(other);//��ƼŬ Play�ϸ� ������Ʈ ���� ��� ��ƼŬ�� �����
            chargeBall.Stop();
        }
        #endregion
        else if (other.gameObject.tag == "AbsoluteAttack") Hitby(other);
    }

    private void OnDisable()=>isAttack = false;

    #region ���� �ִϸ��̼� �̺�Ʈ
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
