using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ConnectionManager : MonoBehaviourPunCallbacks
{
    //InputNickname
    public InputField inputNickname;

    //BtnConnect
    public Button btnConnect;

    void Start()
    {
        // inputNickname 의 내용이 변경될 때 호출되는 함수 등록
        inputNickname.onValueChanged.AddListener(OnValueChanged);

        // inputNickName 에 내용을 작성하다가 Enter를 눌렀을 때 호출되는 함수 등록
        inputNickname.onSubmit.AddListener(OnSubmit);

        // inputNickeName 에 내용을 작성하다가 Focusing 을 잃어버릴 때 호출되는 함수 등록
        inputNickname.onEndEdit.AddListener(
            (string s) => 
            {
                print(s);
            }
        );

        // btnConnect 이 클릭 되었을 때 호출되는 함수 등록
        //btnConnect.onClick.AddListener(OnClickConnect);
    }

    void OnValueChanged(string s)
    {
        btnConnect.interactable = s.Length > 0;

        ////만약에 s 의 길이가 0보다 크면
        //if (s.Length > 0)
        //{
        //    //btnConnect 을 활성화
        //    btnConnect.interactable = true;
        //}
        ////그렇지 않으면
        //else
        //{
        //    //btnConnect 을 비활성화
        //    btnConnect.interactable = false;
        //}
    }
    void OnSubmit(string s)
    {
        print("OnSubmit : " + s);
    }

    public void OnClickConnect()
    {
        // 마스터 서버 접속 요청
        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버에 접속이 성공했으면 호출되는 함수
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("마스터 서버 접속 성공!");

        // 나의 Nickname 을 Photon 에 설정
        PhotonNetwork.NickName = inputNickname.text;
        // Lobby 진입
        PhotonNetwork.JoinLobby();
    }

    // Lobby 진입을 성공했으면 호출되는 함수
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        // 로비 씬으로 이동
        //데이터 유실이 될 수 있기 때문에 포톤에서 제공하는 것을 사용할 것 -> 에러가 나는 순간이 있음
        PhotonNetwork.LoadLevel("LobbyScene");

        print("로비 진입 성공");
    }


    void Update()
    {
        
    }

}
