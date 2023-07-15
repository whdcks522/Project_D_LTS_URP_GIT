using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    
    PhotonView photonView;
    TrailRenderer trailRenderer;
    GameManager gameManager;

    public int dmg;
    public float lifeTime;
    public bool isBullet;
    
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        trailRenderer = GetComponent<TrailRenderer>();
        gameManager = GameManager.Instance;
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
    private void OnEnable()
    {
        if (isBullet && gameManager.photonView.IsMine) Invoke("TimeOver", lifeTime);//�Ѿ� ���� �� ���� �ð� �� ����
    }
    void TimeOver() { photonView.RPC("BulletOff", RpcTarget.AllBuffered); }
    
    [PunRPC]
    public void BulletOff() => gameObject.SetActive(false);
    [PunRPC]
    public GameObject Allreturn() 
    {
        trailRenderer.Clear();
        gameObject.SetActive(true);
        return this.gameObject;
    }

}
