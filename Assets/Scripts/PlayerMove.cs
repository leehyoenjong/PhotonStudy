using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviourPun, IPunObservable, IDamaged
{
    [Header("이동속도")]
    [SerializeField] float moveSpeed;

    [Header("캐릭터 컨트롤러")]
    CharacterController M_CharacterController;

    //y관련 속력
    float yVelocity = 0;

    [Header("점프 파워")]
    [SerializeField] float JumpPower;

    [Header("보간 속력")]
    [SerializeField] float LefpSpeed;

    [Header("닉네임 텍스트")]
    [SerializeField] Text T_NickName;

    [Header("HP UI")]
    [SerializeField] Image Img_Hp;

    //Animator을 가져오자
    [SerializeField] Animator M_Ani;
    float moveV, moveH;



    //최대 HP
    int maxHp = 100;
    //현재 HP
    int currentHp = 0;

    //중력
    float gravity = -9.81f;

    //점프횟수 제한
    int MaxJumpCount = 2;
    //현재 점프 횟수
    int CurrentJumpCount;

    //서버에서 주는 위치값
    Vector3 receivePos;
    //서버에서 주는 회전값
    Quaternion receiveRot;





    /// <summary>
    /// 지속적으로 주고받는 값들에 사용되는 함수
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //만약에 데이터를 보낼 수 있는 상태라면
        //isMine이 true면 IsWriting이 true
        if (stream.IsWriting)
        {
            // 나의 위치 값을 보낸다.
            stream.SendNext(transform.position);
            // 나의 회전 값을 보낸다.
            stream.SendNext(transform.rotation);
            //나의 v값을 보낸다
            stream.SendNext(M_Ani.GetFloat("Vertical"));
            //나의 h값을 보낸다
            stream.SendNext(M_Ani.GetFloat("Horizontal"));
        }
        //그렇지 않다면(받을 수 있는 상태)
        if (stream.IsReading)
        {
            //보낸 순서대로 읽는다//

            //서버에서 위치 값을 받는다.
            receivePos = (Vector3)stream.ReceiveNext();
            //서버에서 회전값을 받는다.
            receiveRot = (Quaternion)stream.ReceiveNext();
            //서버에서 v 값을 받는다.
            moveV = (float)stream.ReceiveNext();
            //서버에서 h 값을 받는다.
            moveH = (float)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Gamemanager.instance.AddPlayer(photonView);

        currentHp = maxHp;

        //해당 플레이어의 닉네임을 설정하자
        T_NickName.text = photonView.Owner.NickName;

        //내 것일때만 
        if (photonView.IsMine)
        {
            M_CharacterController = GetComponent<CharacterController>();
            
            if (M_CharacterController == null)
            {
                M_CharacterController = gameObject.AddComponent<CharacterController>();
            }

            //최초 앞 방향을 spawnCenter을 보겧 ㅏ자
            var forward = -transform.position;
            forward.y = 0;
            transform.forward = forward;


            //닉네임 색을 랜덤하게 뽑아서 다른 사람들에게 알려주자.
            Color nickColor = Random.ColorHSV();
            UI_Chat.instance.nickColor = nickColor;

            //RPC는 클래스는 안넘어간다
            photonView.RPC(nameof(RPCSetNickColor), RpcTarget.AllBuffered, nickColor.r, nickColor.g, nickColor.b);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //만약에 내것 일때만 
        if (photonView.IsMine)
        {
            //WASD 키로
            //Input.GetAxisRaw 바로 탁탁
            //Input.GetAxis 천천히 가는느낌
            var h = Input.GetAxisRaw("Horizontal");
            var v = Input.GetAxisRaw("Vertical");

            //방향 정하고
            var dirH = transform.right * h;
            var dirV = transform.forward * v;
            var dir = dirH + dirV;
            dir.Normalize();

            //만약에 땅에 닿아 있다면
            if (M_CharacterController.isGrounded)
            {
                //만약에 점프 중일때만 
                if (CurrentJumpCount > 0)
                {
                    //Move 애니메이션으로 실행해라
                    photonView.RPC(nameof(RpcSetTrigger), RpcTarget.All, "Land");
                }
                //yVelocity를 0으로 하자
                yVelocity = 0;
                //점프 횟수를 0으로
                CurrentJumpCount = 0;

            }

            //만약에 스페이스바를 누르면
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //만약에 점프 횟수가 최대 점프 횟수보다 많으면
                if (CurrentJumpCount < MaxJumpCount)
                {
                    //yVelocity에 초기 값을 설정
                    yVelocity = JumpPower;
                    //점프 횟수 증가
                    CurrentJumpCount++;
                    photonView.RPC(nameof(RpcSetTrigger), RpcTarget.All, "Jump");
                }
            }
            //yVelocity 를 중력에 의해 중력 만큼 감소 시킨다.
            yVelocity += gravity * Time.deltaTime;

            //dir 의 y값에 yVelocity를 셋팅
            dir.y = yVelocity;


            //그 방향으로 계속 이동
            //transform.position += dir * moveSpeed * Time.deltaTime;
            M_CharacterController.Move(dir * moveSpeed * Time.deltaTime);

            //h,v 값을 애니메이션 파라메타 값으로 전달

            moveV = Input.GetAxis("Vertical");
            moveH = Input.GetAxis("Horizontal");
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, receivePos, LefpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, receiveRot, LefpSpeed * Time.deltaTime);
        }
        M_Ani.SetFloat("Vertical", moveV);
        M_Ani.SetFloat("Horizontal", moveH);

        //hp바 스무스하게 갱신
        Img_Hp.fillAmount = Mathf.Lerp(Img_Hp.fillAmount, (float)currentHp / maxHp, Time.deltaTime * 10);
    }

    [PunRPC]
    void RPCSetNickColor(float r, float g, float b)
    {
        T_NickName.color = new Color(r, g, b, 1);
    }

    [PunRPC]
    void RpcUpdateHp(int damage)
    {
        currentHp += damage;
        if (currentHp < 0)
        {
            currentHp = 0;
        }
        //Hp바 갱신
        //Img_Hp.fillAmount = (float)currentHp / maxHp;


        //print(photonView.Owner.NickName + "의 현재 체력 " + currentHp);
    }

    [PunRPC]
    void RpcSetTrigger(string param)
    {
        M_Ani.SetTrigger(param);
    }


    public void UpdateHp(int damage)
    {
       photonView.RPC(nameof(RpcUpdateHp),RpcTarget.All,damage);
    }


    public void OnDamaged(int damage)
    {
       UpdateHp(damage);
    }

    [PunRPC]
    public void fdsafasd()
    {

    }
}