using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviourPunCallbacks
{
    
    public PhotonView photonView;//public 내비두자
    public TrailRenderer trailRenderer;
    GameManager gameManager;
    Rigidbody rigid;

    public int dmg;
    public int speed;
    public float lifeTime;
    public bool isBullet;//충돌 시 사라질 것인가
    public bool isGenetic;//선천적인 것인가?
    
    
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        trailRenderer = GetComponent<TrailRenderer>();
        gameManager = GameManager.Instance;
        rigid = GetComponent<Rigidbody>();

        //선천적인 것이 아니면 부모를 게임매니저 안에
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
            Invoke("TimeOver", lifeTime);//총알 생성 시 일정 시간 후 삭제
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
