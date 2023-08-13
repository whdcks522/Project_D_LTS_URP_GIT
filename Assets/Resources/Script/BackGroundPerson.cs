using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundPerson : MonoBehaviour
{
    SkinnedMeshRenderer[] skinnedMeshRenderer = new SkinnedMeshRenderer[2];
    public TrailRenderer[] trailRenderer = new TrailRenderer[2];

    Vector3 replacePos = new Vector3(-900, -343, 95);
    private void Awake()
    {
        //초기화
        skinnedMeshRenderer[0] = gameObject.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer[1] = gameObject.transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();

        //1번 관절
        skinnedMeshRenderer[0].material.SetColor("_ColorControl", new Color(0.6117647f, 0.3882353f, 0.3490196f, 1));
        //2번 몸체
        skinnedMeshRenderer[1].material.SetColor("_ColorControl", new Color(0.9254902f, 0.5843138f, 0.5490196f, 1));
    }

    void FixedUpdate()
    {
        //걸어서 이동
        transform.position += Vector3.left * 1;
        //좌측으로 가면 순간이동
        if (transform.position.x < -1200) 
        {
            transform.position = replacePos;
            transform.position += Vector3.right * Random.Range(50, 100) ;

            trailRenderer[0].Clear();
            trailRenderer[1].Clear();
        }
    }
}
