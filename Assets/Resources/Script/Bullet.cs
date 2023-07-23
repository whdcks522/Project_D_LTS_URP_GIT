using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviourPunCallbacks
{
    
    public PhotonView photonView;//public �������
    public TrailRenderer trailRenderer;
    GameManager gameManager;
    Rigidbody rigid;

    public int dmg;
    public int speed;
    public float lifeTime;
    public bool isBullet;//�浹 �� ����� ���ΰ�
    public bool isGenetic;//��õ���� ���ΰ�?
    
    
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
        if (lifeTime != 0)
        {
            Invoke("TimeOver", lifeTime);//�Ѿ� ���� �� ���� �ð� �� ����
        } 
    }

    private void FixedUpdate()
    {
        if (tag == "PlayerAttack") 
        {
            transform.Rotate(Vector3.up * 450 * Time.deltaTime);//right

        }
    }

    void TimeOver() {
       photonView.RPC("BulletOff", RpcTarget.AllBuffered); 
    }

    [PunRPC]
    public void BulletOff() 
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void RPCActivate(Vector3 vec)
    {
        gameObject.SetActive(true);
        rigid.velocity = vec * speed;
    }
}
