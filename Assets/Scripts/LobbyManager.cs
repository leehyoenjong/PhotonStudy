using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    // Input Room Name
    public InputField inputRoomName;
    // Input Password Name
    public InputField inputPassword;
    // Input Max Player
    public InputField inputMaxPlayer;
    // 방 생성 버튼
    public Button btnCreateRoom;
    // 방 참여 버튼
    public Button btnJoinRoom;
    // RoomListItem Prefab
    public GameObject roomListItemFactory;
    // ScrolView -> Content 의 Transform
    public Transform rtContent;

    // 방 목록을 가지고 있는 변수
    Dictionary<string, RoomInfo> dicRoomInfo = new Dictionary<string, RoomInfo>();

    // 방 목록의 변화가 있을때 호출되는 함수
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //변경된 사항만 업데이트 함
        //처음 생성된 애들은 업데이트 하지 않음


        base.OnRoomListUpdate(roomList);

        // ScrollView -> Content 에 자식으로 붙어있는 Item 을 다 삭제하자.
        DeleteRoomListItem();

        // dicRoomInfo 변수를 roomList 를 이용해서 갱신
        UpdateRoomListItem(roomList);

        // dicRoomInfo 을 기반으로 roomListItem 을 만들자
        CreateRoomListItem();


    }

    void DeleteRoomListItem()
    {
        for (int i = 0; i < rtContent.childCount; i++)
        {
            Destroy(rtContent.GetChild(i).gameObject);
        }
    }

    void UpdateRoomListItem(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // dicRoomInfo 에 info 의 방이름으로 되어있는 Key 값이 존재하니?
            if (dicRoomInfo.ContainsKey(info.Name))
            {
                // 수정 or 삭제
                //만약에 이방이 삭제 되었니?
                if (info.RemovedFromList)
                {
                    // 삭제
                    dicRoomInfo.Remove(info.Name);
                    continue;
                }
            }

            // 추가 or 삭제
            dicRoomInfo[info.Name] = info;
        }
    }

    void CreateRoomListItem()
    {
        foreach (RoomInfo info in dicRoomInfo.Values)
        {
            // RoomListItem 을 생성과 동시에 ScrollView -> Content 의 자식으로 하자
            GameObject go = Instantiate(roomListItemFactory, rtContent);
            // 생성된 item 에서 RoomListItem 컴포넌트를 가져온다.
            RoomListItem item = go.GetComponent<RoomListItem>();
            // 가져온 컴포넌트가 가지고 있는 SetInfo 함수를 실행
            item.SetInfo(info);
            // item 이 클릭 되었을 때 호출되는 함수 등록
            item.onDelegate = SelectRoomItem;
        }
    }

    void SelectRoomItem(string roomName)
    {
        inputRoomName.text = roomName;
    }

    void Start()
    {
        //방 이름이 변경될 때 호출되는 함수 등록
        inputRoomName.onValueChanged.AddListener(OnValueChangeRoomName);
        //최대 인원이 변경될 때 호출되는 함수 등록
        inputMaxPlayer.onValueChanged.AddListener(OnValueChangeMaxPlayer);

        //생성 버튼 눌렀을 때 호출되는 함수 등록
        btnCreateRoom.onClick.AddListener(OnClickCreateRoom);
        //참여 버튼 눌렀을 때 호출되는 함수 등록
        btnJoinRoom.onClick.AddListener(OnClickJoinRoom);
    }

    void Update()
    {

    }

    // 참여 & 생성 버튼에 관여
    void OnValueChangeRoomName(string roomName)
    {
        // 참여 버튼 활성 / 비활성
        btnJoinRoom.interactable = roomName.Length > 0;

        // 생성 버튼 활성 / 비활성
        btnCreateRoom.interactable = roomName.Length > 0 && inputMaxPlayer.text.Length > 0;
    }

    // 생성 버튼에 관여
    void OnValueChangeMaxPlayer(string maxPlayer)
    {
        // 생성 버튼 활성 / 비활성
        btnCreateRoom.interactable = maxPlayer.Length > 0 && inputRoomName.text.Length > 0;
    }

    // 생성 버튼 클릭시 호출되는 함수
    void OnClickCreateRoom()
    {
        // 방 옵션을 설정 
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = int.Parse(inputMaxPlayer.text);
        // 방목록에 보이게 할것인가?
        options.IsVisible = true;
        // 플레이 중 방에 참여 가능 여부
        options.IsOpen = true;

        //custom 설정
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash["room_name"] = inputRoomName.text;
        hash["use_item"] = true;
        options.CustomRoomProperties = hash;

        //custom 설정한 것중에 lobby에서 보여주고 싶은 키값을 설정
        string[] customkeys = { "room_name", "use_item" };
        options.CustomRoomPropertiesForLobby = customkeys;

        // 방 생성
        PhotonNetwork.CreateRoom(inputRoomName.text + inputPassword.text, options);
    }


    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("방 생성 성공");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("방 생성 실패 : " + message);
    }

    // 참여 버튼 클릭시 호출되는 함수
    void OnClickJoinRoom()
    {
        // 방 참여
        PhotonNetwork.JoinRoom(inputRoomName.text + inputPassword.text);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("방 입장 성공");

        //게임 하는 씬으로 이동
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("방 입장 실패 : " + message);
    }

    //방이 있다면 입장 없다면 생성
    void JoinOrCreateRoom()
    {
        // 방 옵션을 설정 
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = int.Parse(inputMaxPlayer.text);
        // 방목록에 보이게 할것인가?
        options.IsVisible = true;
        // 방에 참여 가능 여부
        options.IsOpen = true;

        PhotonNetwork.JoinOrCreateRoom(inputRoomName.text, options, TypedLobby.Default);
    }

    //랜덤으로 방 입장
    void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
    }


}
