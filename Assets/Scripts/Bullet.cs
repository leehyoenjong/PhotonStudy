using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    [Header("리지드 바디")]
    [SerializeField] Rigidbody M_Rigidbody;
    [Header("속력")]
    [SerializeField] float Speed;

    [Header("폭발 효과 공장")]
    [SerializeField] GameObject G_Explofactory;

    // Start is called before the first frame update
    void Start()
    {
        M_Rigidbody = this.GetComponent<Rigidbody>();
        //자신의 앞방향을 향하는 속도 설정
        M_Rigidbody.velocity = transform.forward * Speed;
    }

    // Update is called once per frame 
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        photonView.RPC(nameof(RpcShowExplo), RpcTarget.All, transform.position);


    }

    [PunRPC]
    void RpcShowExplo(Vector3 vec)
    {
        // 폭발 파티클 효과 만들자
        var explo = Instantiate(G_Explofactory);
        // 만들어진 폭발 효과를 나의 위치에 위치 시키자.
        explo.transform.position = vec;

        //나를 파괴하자
        Destroy(this.gameObject);
    }
}