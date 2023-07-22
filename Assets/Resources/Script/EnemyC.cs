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

        //�ְ����� ���� ������ 
        skinnedMeshRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
    }
    private void FixedUpdate()
    {
        if (health > 0 && anim.GetBool("isLive") && !isAttack)//���� ���̸鼭, ���������� ������//Bars != null && Bars.activeSelf(�߰� 2��)
        {
                //�þ� ����
                transform.LookAt(target.transform.position);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                //���� ó���� ����
                isAttack = true;
                //�þ߸� ����
                isAttack2 = true;
                //��ġ ����
                attackPos = transform.position + (target.transform.position - transform.position).normalized * 2f;
                //����ȭ
                InvisibleDissolve();
                //�ִϸ��̼� ����
                anim.SetTrigger("isAttack");
        }
        else if (!isAttack2 && health > 0)
        {
            //�þ� ����
            transform.LookAt(target.transform.position);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }

    private void OnTriggerEnter(Collider other)//���� �浹��
    {
            Hitby(other);
        if (other.gameObject.tag == "PlayerAttack" && !isDissolve) //�÷��̾� ����
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

    #region ���� �ִϸ��̼� �̺�Ʈ

    public void AttackStart()
    {
        if (health > 0)
        {
            //���� ó��
            earthQuake.Play();
            earthCol.enabled = true;
        }
        else isAttack = false;
    }

    void AttackStay() => //���� ó��
            earthCol.enabled = false;

    public void AttackContinue() 
    {
        if (health > 0)
        {
              
            //���� ��ġ �̵�
            transform.position = attackPos;
            isAttack2 = false;
            //���� ������ ���� ���
            CancelInvoke();
            Invoke("AttackEnd", 2f);
        }
        else isAttack = false;
    }

    void AttackEnd() => isAttack = false;
     
    #endregion
}
