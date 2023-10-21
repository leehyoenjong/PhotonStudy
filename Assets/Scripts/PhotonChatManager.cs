using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;


public class PhotonChatManager : MonoBehaviour, IChatClientListener
{
    //input chat
    public InputField inputchat;

    public GameObject chatItemFactory;

    public RectTransform rtContent;

    //채팅서버 초기 셋팅
    ChatAppSettings chatAppSettings;

    //Chatitemprefab
    //scrollview의 Content의 RectTransform

    //ChatClient
    ChatClient chatClient;

    void Start()
    {
        //inputchat에서 엔터쳤을 때 호출되는 함수 등록
        inputchat.onSubmit.AddListener(OnSubmit);

        //채팅서버 초기 설정
        PhotonChatSetting();

        //채팅서버 접속
        PhotonChatConnet();
    }


    void Update()
    {
        //만약에 chatClient가 값이 있으면
        if (chatClient != null)
        {
            chatClient.Service();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChatChannel chat = null;
            bool found = chatClient.TryGetChannel("대구", out chat);

            if (found)
            {
                string str = chat.ToStringMessages();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            chatClient.SendPrivateMessage(inputchat.text, "안녕하세요");
        }
    }


    void OnSubmit(string s)
    {
        //\msg 아이디 메시지
        if (s.Contains("\\msg"))
        {
            var chat = s.Split(" ",3);
            chatClient.SendPrivateMessage(chat[1], chat[2]);
        }
        else
        {
            //채팅 서버에게 채팅을 보내자
            chatClient.PublishMessage("대구", s);
        }
    }

    void PhotonChatSetting()
    {
        //포톤 설정을 가져와서 ChatAppSettings에 설정
        AppSettings settings = PhotonNetwork.PhotonServerSettings.AppSettings;
        chatAppSettings = new ChatAppSettings();
        chatAppSettings.AppIdChat = settings.AppIdChat;
        chatAppSettings.AppVersion = settings.AppVersion;
        chatAppSettings.FixedRegion = settings.FixedRegion;
        chatAppSettings.AppVersion = settings.AppVersion;
        chatAppSettings.NetworkLogging = settings.NetworkLogging;
        chatAppSettings.Protocol = settings.Protocol;
        chatAppSettings.EnableProtocolFallback = settings.EnableProtocolFallback;
        chatAppSettings.Server = settings.Server;
        chatAppSettings.Port = (ushort)settings.Port;
        chatAppSettings.ProxyServer = settings.ProxyServer;
    }

    void PhotonChatConnet()
    {
        chatClient = new ChatClient(this);
        //채팅할때 NickName설정
        chatClient.AuthValues = new Photon.Chat.AuthenticationValues(PhotonNetwork.NickName);
        //설정을 이용하여 연결 시도
        chatClient.ConnectUsingSettings(chatAppSettings);
    }


    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnDisconnected()
    {

    }

    public void OnConnected()
    {
        print("챗 접속 완료");
        //채널을 만들자.
        chatClient.Subscribe("대구");
    }


    public void OnChatStateChange(ChatState state)
    {

    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            var go = Instantiate(chatItemFactory, rtContent);
            UI_ChatBar chatBar = go.GetComponent<UI_ChatBar>();
            chatBar.SetInit(senders[i] + " : " + messages[i],Color.black);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        print(sender + " : " + message + " 채널 : " + channelName);
        var go = Instantiate(chatItemFactory, rtContent);
        UI_ChatBar chatBar = go.GetComponent<UI_ChatBar>();
        chatBar.SetInit(sender + " : " + message, Color.red);
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            print("채팅 채널 개설[" + channels[i] + "]" + " 추가 성공");
        }
    }

    public void OnUnsubscribed(string[] channels)
    {

    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }

    public void OnUserSubscribed(string channel, string user)
    {

    }

    public void OnUserUnsubscribed(string channel, string user)
    {

    }
}
