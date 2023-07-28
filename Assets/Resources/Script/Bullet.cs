using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class Bullet : MonoBehaviourPunCallbacks
{
    
    public PhotonView photonView;//public �������
    public TrailRenderer trailRenderer;
    GameManager gameManager;
    Rigidbody rigid;

    public int dmg;
    public int speed;
    public int secondSpeed;//����ġ
    public float lifeTime;
    public float secondLifeTime;
    public bool isBullet;//�浹 �� ����� ���ΰ�
    public bool isGenetic;//��õ���� ���ΰ�?
    public Enemy parent;
    public enum BulletType { Normal, Accel ,Curve }
    public BulletType bulletType;


    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        trailRenderer = GetComponent<TrailRenderer>();
        gameManager = GameManager.Instance;
        rigid = GetComponent<Rigidbody>();

        //��õ���� ���� �ƴϸ� �θ� ���ӸŴ��� �ȿ�
        if (!isGenetic)
        transform.parent = gameManager.transform;
    }

    private void OnDisable()=>CancelInvoke();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "AbsoluteAttack") gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (lifeTime != 0)//������ 0�� �ƴѰ�� ���� �ð� �� ����
        {
            Invoke("TimeOver", lifeTime);
        }
    }

    void Accel() 
    {
        photonView.RPC("RPCAccel", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPCAccel()
    {
        rigid.velocity *= 5;
    }

    void TimeOver() //���� ��, ó������ ������ �� �� ���
    {
        //���� �Ѿ� ����
        if (BulletType.Normal == bulletType)
            photonView.RPC("BulletOff", RpcTarget.AllBuffered);

        #region ������ ��� ���� ����
        else if (BulletType.Accel == bulletType)
        {
            rigid.velocity *= secondSpeed;

            //�ݹ� ����
            Invoke("BulletOffStart", secondLifeTime);

            //����
            GameObject bullet = gameManager.Get("EnemyBulletB");
            //���� ��ġ ���� 
            bullet.transform.position = transform.position;
            //���� ��Ʈ��ũ
            bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
        }
        #endregion

        #region Ŀ���� ��� ���� ����
        else if (BulletType.Curve == bulletType) 
        {
            //�θ� ���� Ÿ���� ��ġ Ȯ��
            Vector3 targetPos = parent.target.transform.position;
            //����ü ���� ����
            transform.rotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            //����ü ��Ʈ��ũ�� ���� ���� ����
            photonView.RPC("RPCActivate", RpcTarget.AllBuffered, transform.forward);
            //����ü ���� ������
            transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, 0);
            //�ݹ� ����
            Invoke("BulletOffStart", secondLifeTime);

            //����
            GameObject bullet = gameManager.Get("EnemyBulletB");
            //���� ��ġ ���� 
            bullet.transform.position = transform.position;
            //���� ��Ʈ��ũ
            bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
        }
        #endregion
    }

    void BulletOffStart()//������ ����
    {
        photonView.RPC("BulletOff", RpcTarget.AllBuffered);
    }

        [PunRPC]
    public void BulletOff() //�Ѿ� ��Ȱ��ȭ
    {
        gameObject.SetActive(false);
    }

    
    [PunRPC]//�Ѿ� Ȱ��ȭ ��, ���� ����
    public void RPCActivate(Vector3 vec)
    {
        gameObject.SetActive(true);
        rigid.velocity = vec * speed;
    }
}
