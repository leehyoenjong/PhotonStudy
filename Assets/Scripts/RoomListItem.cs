using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    // 정보를 표시하는 Text 컴포넌트
    public Text roomInfo;

    // 클릭 되었을 때 호출되는 함수 
    public Action<string> onDelegate;

    public void SetInfo(RoomInfo info)
    {
        // 나의 게임오브젝의 이름을 roomName 으로 하겠다
        name = info.CustomProperties["room_name"].ToString();

        //아이템전 여부
        var useitem = (bool)info.CustomProperties["use_item"];
        string struseitem = useitem == true ? "템전" : "노템";
        // roomInfo 내용을 설정
        roomInfo.text = name + " (" + info.PlayerCount + " / " + info.MaxPlayers + ")" + "\t" + struseitem;
    }

    public void OnClick()
    {
        //만약에 onDelegate 에 무언가 들어있다면
        if(onDelegate != null)
        {
            //onDelegate 를 실행하자.
            onDelegate(name);
        }

        //// InputRoomName 찾아오자.
        //GameObject go = GameObject.Find("InputRoomName");
        //// 찾아온 게임오브젝트에서 InputField 컴포넌트를 가져오자
        //InputField inputField = go.GetComponent<InputField>();
        //// 가져온 컴포넌트의 text 값을 나의 이름으로 셋팅하자.
        //inputField.text = name;
    }
}
