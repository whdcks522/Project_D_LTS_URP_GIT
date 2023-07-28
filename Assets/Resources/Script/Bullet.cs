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
    
    public PhotonView photonView;//public 내비두자
    public TrailRenderer trailRenderer;
    GameManager gameManager;
    Rigidbody rigid;

    public int dmg;
    public int speed;
    public int secondSpeed;//가속치
    public float lifeTime;
    public float secondLifeTime;
    public bool isBullet;//충돌 시 사라질 것인가
    public bool isGenetic;//선천적인 것인가?
    public Enemy parent;
    public enum BulletType { Normal, Accel ,Curve }
    public BulletType bulletType;


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
        if (lifeTime != 0)//수명이 0이 아닌경우 일정 시간 후 삭제
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

    void TimeOver() //생성 후, 처음으로 수명이 다 될 경우
    {
        //기존 총알 종료
        if (BulletType.Normal == bulletType)
            photonView.RPC("BulletOff", RpcTarget.AllBuffered);

        #region 가속일 경우 새로 생성
        else if (BulletType.Accel == bulletType)
        {
            rigid.velocity *= secondSpeed;

            //금방 삭제
            Invoke("BulletOffStart", secondLifeTime);

            //폭발
            GameObject bullet = gameManager.Get("EnemyBulletB");
            //폭발 위치 조정 
            bullet.transform.position = transform.position;
            //폭발 네트워크
            bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
        }
        #endregion

        #region 커브일 경우 새로 생성
        else if (BulletType.Curve == bulletType) 
        {
            //부모를 통해 타겟의 위치 확인
            Vector3 targetPos = parent.target.transform.position;
            //투사체 방향 조정
            transform.rotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            //투사체 네트워크를 통한 가속 조정
            photonView.RPC("RPCActivate", RpcTarget.AllBuffered, transform.forward);
            //투사체 방향 재조정
            transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, 0);
            //금방 삭제
            Invoke("BulletOffStart", secondLifeTime);

            //폭발
            GameObject bullet = gameManager.Get("EnemyBulletB");
            //폭발 위치 조정 
            bullet.transform.position = transform.position;
            //폭발 네트워크
            bullet.GetComponent<Bullet>().photonView.RPC("RPCActivate", RpcTarget.AllBuffered, Vector3.zero);
        }
        #endregion
    }

    void BulletOffStart()//정말로 종료
    {
        photonView.RPC("BulletOff", RpcTarget.AllBuffered);
    }

        [PunRPC]
    public void BulletOff() //총알 비활성화
    {
        gameObject.SetActive(false);
    }

    
    [PunRPC]//총알 활성화 후, 방향 조정
    public void RPCActivate(Vector3 vec)
    {
        gameObject.SetActive(true);
        rigid.velocity = vec * speed;
    }
}
