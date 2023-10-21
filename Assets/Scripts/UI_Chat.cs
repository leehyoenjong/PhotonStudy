using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

public class UI_Chat : MonoBehaviourPun
{
    public static UI_Chat instance;

    [Header("인풋 필드")]
    [SerializeField] InputField inputtext;
    [Header("쳇 오브젝트")]
    [SerializeField] GameObject G_Chat;
    [Header("생성 위치")]
    [SerializeField] RectTransform Rt_Content;

    [SerializeField] RectTransform Rt_ScrollView;

    //닉네임 컬러
    public Color nickColor;
    //채팅 추가 전 Content의 H 값
    float prevContentH = 0;


    private void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        //마우스 포인터 비활성화
        Cursor.visible = false;

        //채팅 내용을 치고 엔터를 치면 호출되는 함수 등록
        inputtext.onSubmit.AddListener(OnSumit);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Cursor.visible = true;
        }
        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
            //모바일 UI 터치
            //EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId);
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Cursor.visible = false;
            }
        }
    }

    void OnSumit(string str)
    {
        //만약에 str이 없으면 함수 나가자
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        //ColorUtility.ToHtmlStringRGB(nickColor)  컬러 값을 16진수로 바꾸는 함수   
        var chat = "<color=#" + ColorUtility.ToHtmlStringRGB(nickColor) + ">" + PhotonNetwork.NickName + "</color> : " + str;

        photonView.RPC(nameof(RpcAddChat), RpcTarget.All, chat);

        //inputfild 값 초기화
        inputtext.text = "";
        //inputField의 강제 Foucs를 해준다.
        inputtext.ActivateInputField();
    }

    [PunRPC]
    void RpcAddChat(string s)
    {
        prevContentH = Rt_Content.sizeDelta.y;

        //chatitem을 하나 만든다.
        //만들어진 item을 ScorllView의 Content의 자식으로 하자
        //itme을 위치시키자.
        var chat = Instantiate(G_Chat, Rt_Content).GetComponent<UI_ChatBar>();
        var addHeight = chat.SetInit(s, Rt_Content.sizeDelta.y);

        //Content의 크기를 추가된 item의 Height 만큼 증가
        Rt_Content.sizeDelta += new Vector2(0, addHeight);

        AutoScroll();
    }

    void AutoScroll()
    {
        //ScrollView의 H보다 Content의 H값이 크다면(스크롤 가능 상태)
        if(Rt_ScrollView.sizeDelta.y < Rt_Content.sizeDelta.y)
        {
            //만약에 ScrollView의 Content가 바닥에 닿아있다면
            var Yvalue = prevContentH - Rt_ScrollView.sizeDelta.y;
            var check = Yvalue <= Rt_Content.anchoredPosition.y;
            if (check)
            {
                var sizeY = Rt_Content.sizeDelta.y - Rt_ScrollView.sizeDelta.y;
                Rt_Content.anchoredPosition = new Vector2(0, sizeY);
            }
        }
    
    }
}