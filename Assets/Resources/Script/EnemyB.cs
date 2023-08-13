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

        //�θ� ����
        transform.parent = gameManager.transform;
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
        if (health > 0 )
        {
            if (gameManager.photonView.IsMine)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    GameObject bullet = gameManager.Get("EnemyBulletA");
                    //����ü ��ġ ���� 
                    bullet.transform.position = transform.position + Vector3.up + transform.forward.normalized * 1f;
                    //����ü ���� ����
                    bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + i * 20, 0);
                    //����ü ��Ʈ��ũ�� ���� ���� ����
                    bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, bullet.transform.forward);
                    //����ü ���� ������
                    bullet.transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y + i * 20, 0);

                    //��ƼŬ
                    ParticleSystem bulletParticle = bullet.GetComponent<ParticleSystem>();
                    bulletParticle.Stop();
                    bulletParticle.Simulate(2f); // ���ÿ����� 1�ʷ� �����Ͽ� �̹� 2�� ����� ���·� ����
                                                 //bulletParticle.Play(); //ȸ�� ��ü�� ���͸����� �ϹǷ� �ʿ����(���÷� ó���ϴϱ� �� ���������)
                }
            }
        }
        else isAttack = false;
    }


    void AttackEnd() => isAttack = false;


    #endregion
}
