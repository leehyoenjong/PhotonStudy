using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager instance;

    //Spawn Pos 들
    [SerializeField] Vector3[] SpawnPos;
    //Spawn Canter
    [SerializeField] Transform tr_SpawnCenter;

    //모든 플레이어의 PhotonView를 담을 리스트
    public List<PhotonView> L_PlayerList = new List<PhotonView>();

    int CurrentTuneIdx = -1;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //OnPhotoenSerialview 호출 빈도
        PhotonNetwork.SerializationRate = 30;

        //RPC 전송 빈도 1초에 30번 모아서 보내겠다,
        PhotonNetwork.SendRate = 30;

        //Spawn 위치들 꼐산하자
        SetSpawnPos();

        var idx = PhotonNetwork.CurrentRoom.PlayerCount - 1;

        // 내 플레이어 생성(방에 접속된 인원들한테도 나의 캐릭터를 생성)
        PhotonNetwork.Instantiate("Player", SpawnPos[idx], Quaternion.identity);
    }

    void Update()
    {

    }

    void SetSpawnPos()
    {
        SpawnPos = new Vector3[PhotonNetwork.CurrentRoom.MaxPlayers];
        var angle = 360.0f / SpawnPos.Length;

        for (int i = 0; i < SpawnPos.Length; i++)
        {
            SpawnPos[i] = tr_SpawnCenter.position + tr_SpawnCenter.forward * 2;
            SpawnPos[i].y = 1;
            tr_SpawnCenter.Rotate(0, angle, 0);
            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //go.transform.position = SpawnPos[i];
        }
    }

    public void AddPlayer(PhotonView pv)
    {
        L_PlayerList.Add(pv);
        //만약 max 인원과 all플레이어의 갯수가 같을 경우(다 들어옴)
        if (IsAllEnter())
        {
            ChangeTurn();
            //NextTune();
        }
    }


    public void ChangeTurn()
    {
        //방장이 아니면 함수를 나가자
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PlayerFire fire;
        //이전의 쏠 수 있었 던 사람을 못쏘게 하자
        if (CurrentTuneIdx != -1)
        {
            fire = L_PlayerList[CurrentTuneIdx].GetComponent<PlayerFire>();
            L_PlayerList[CurrentTuneIdx].RPC(nameof(fire.RpcChangeFire), RpcTarget.All, false);
        }

        //다음 순서
        CurrentTuneIdx++;

        //최대 값이 되면 0이 됨
        CurrentTuneIdx %= L_PlayerList.Count;

        //쏠수 있게 하자
        fire = L_PlayerList[CurrentTuneIdx].GetComponent<PlayerFire>();
        L_PlayerList[CurrentTuneIdx].RPC(nameof(fire.RpcChangeFire), RpcTarget.All, true);
    }

    public bool IsAllEnter()
    {
        return PhotonNetwork.CurrentRoom.MaxPlayers == L_PlayerList.Count;
    }
}