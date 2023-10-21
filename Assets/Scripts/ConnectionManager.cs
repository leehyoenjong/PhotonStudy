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
        // inputNickname �� ������ ����� �� ȣ��Ǵ� �Լ� ���
        inputNickname.onValueChanged.AddListener(OnValueChanged);

        // inputNickName �� ������ �ۼ��ϴٰ� Enter�� ������ �� ȣ��Ǵ� �Լ� ���
        inputNickname.onSubmit.AddListener(OnSubmit);

        // inputNickeName �� ������ �ۼ��ϴٰ� Focusing �� �Ҿ���� �� ȣ��Ǵ� �Լ� ���
        inputNickname.onEndEdit.AddListener(
            (string s) => 
            {
                print(s);
            }
        );

        // btnConnect �� Ŭ�� �Ǿ��� �� ȣ��Ǵ� �Լ� ���
        //btnConnect.onClick.AddListener(OnClickConnect);
    }

    void OnValueChanged(string s)
    {
        btnConnect.interactable = s.Length > 0;

        ////���࿡ s �� ���̰� 0���� ũ��
        //if (s.Length > 0)
        //{
        //    //btnConnect �� Ȱ��ȭ
        //    btnConnect.interactable = true;
        //}
        ////�׷��� ������
        //else
        //{
        //    //btnConnect �� ��Ȱ��ȭ
        //    btnConnect.interactable = false;
        //}
    }
    void OnSubmit(string s)
    {
        print("OnSubmit : " + s);
    }

    public void OnClickConnect()
    {
        // ������ ���� ���� ��û
        PhotonNetwork.ConnectUsingSettings();
    }

    // ������ ������ ������ ���������� ȣ��Ǵ� �Լ�
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("������ ���� ���� ����!");

        // ���� Nickname �� Photon �� ����
        PhotonNetwork.NickName = inputNickname.text;
        // Lobby ����
        PhotonNetwork.JoinLobby();
    }

    // Lobby ������ ���������� ȣ��Ǵ� �Լ�
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        // �κ� ������ �̵�
        //������ ������ �� �� �ֱ� ������ ���濡�� �����ϴ� ���� ����� �� -> ������ ���� ������ ����
        PhotonNetwork.LoadLevel("LobbyScene");

        print("�κ� ���� ����");
    }


    void Update()
    {
        
    }

}
