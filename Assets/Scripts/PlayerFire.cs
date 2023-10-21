using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFire : MonoBehaviourPun
{
    //총알 공장
    [SerializeField] GameObject G_BulletFactory;

    [SerializeField] float Pos;

    //카메라
    Transform TrCam;

    [Header("파편 효과")]
    [SerializeField] GameObject G_BulletImpactFactory;

    public bool Turn;

    void Start()
    {
        TrCam = Camera.main.transform;
    }

    void Update()
    {
        //만약에 내가 쏠수 없으면 함수를 나가자
        //if (!Turn)
        //    return;

        //만약에 마우스가 활성화 되어 있따면 나가자
        if (Cursor.visible)
            return;

        //만약에 내 것이 아니라면 함수를 나가자
        if (!photonView.IsMine)
            return;


        //Q버튼을 누르면 
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //RpcTarget.AllBuffered 늦게 들어와도 늦게 들어온사람도 실행하게 함
            //photonView.RPC(nameof(RpcFire), RpcTarget.All, TrCam.position, TrCam.forward);

            var pos = TrCam.position + TrCam.forward * 1;
            var rot = TrCam.transform.rotation;
            PhotonNetwork.Instantiate("Bullet", pos, rot);
            photonView.RPC(nameof(RpcRequestChangeTurn), RpcTarget.MasterClient);
        }

        // 2번키 누르면
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 카메라 위치, 카메라 앞방향으로 설정된 Ray를 만들자.
            Ray ray = new Ray(TrCam.position, TrCam.forward);

            RaycastHit hitinfo;

            // Ray를 쏘자.
            // out을 이용하면 구조체도 변경 가능
            if (Physics.Raycast(ray, out hitinfo))
            {
                //IDamaged 컴포넌트 가져오자.
                var idamaged = hitinfo.transform.GetComponent<IDamaged>();
                if(idamaged != null)
                {
                    idamaged.OnDamaged(-10);
                    //이렇게 써도 됨
                  //playermove.photonView.RPC(nameof(playermove.UpdateHp));
                }

                photonView.RPC(nameof(RpcShowBulletImpact), RpcTarget.All, hitinfo.point, hitinfo.normal);
                photonView.RPC(nameof(RpcRequestChangeTurn), RpcTarget.MasterClient);
                //var move = GetComponent<PlayerMove>();
                //move.AttackTurnOff();
            }
        }
    }

    [PunRPC]
    void RpcShowBulletImpact(Vector3 point, Vector3 normal)
    {
        // 그 위치에 파편효과를 만들자
        var impact = Instantiate(G_BulletImpactFactory);
        impact.transform.position = point;
        impact.transform.forward = normal;

        //2초뒤에 파괴하자
        Destroy(impact, 2f);
    }

    [PunRPC]
    public void RpcChangeFire(bool isFile)
    {        
        //만약에 내 것이 아니라면 함수를 나가자
        if (!photonView.IsMine)
            return;
        Turn = isFile;
    }

    [PunRPC]
    void RpcRequestChangeTurn()
    {
        Gamemanager.instance.ChangeTurn();
    }



    //[PunRPC]
    //void RpcFire(Vector3 pos, Vector3 forward)
    //{

    //    //총알을 생성
    //    var bullet = Instantiate(G_BulletFactory);
    //    //생성된 총알을 카메라의 앞방향에서 1만큼 떨어진 위치에 위치시키자.
    //    bullet.transform.position = pos + forward * 1;
    //    //생성된 총알의 앞방향을 카메라의 앞방향으로 하자
    //    bullet.transform.forward = forward;
    //}
}
