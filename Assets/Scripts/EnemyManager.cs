using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviourPun
{
    //SpawnPos 들
    [SerializeField] Transform[] SpawnPos;
    //Enemy Prfab
    [SerializeField] GameObject G_EnemyFactory;
    //생성 시간
    public float CreateTime = 2;
    //현재 시간
    float CurrentTime;

    //최대 Enemy 갯수
    public int maxEnemy = 1;
    //현재 Enemy 갯수
    int currEnemy = 0;


    // Start is called before the first frame update
    void Start()
    {
        SpawnPos = GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //방장이 아니라면 함수를 나가자
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        //최대로 생성할 수 있는 Enemy 갯수라면 나가자
        if (currEnemy >= maxEnemy)
        {
            return;
        }


        if (!Gamemanager.instance.IsAllEnter()) return;
        //인원이 다 들어오지 않으면 나가자



        CurrentTime += Time.deltaTime;
        if (CurrentTime > CreateTime)
        {
            CurrentTime = 0;

            currEnemy++;

            //생성 위치
            var idx = Random.Range(1, SpawnPos.Length);
            //랜덤 추가 위치
            var randDist = Random.Range(0f, 5f);
            //랜덤 방향
            var randAngle = Random.Range(0, 360);
            //생성 각도 조절
            var ranrot = Quaternion.Euler(0, randAngle, 0);
            SpawnPos[idx].Rotate(0, randAngle, 0);
            //위치 조절
            var createpos = SpawnPos[idx].position + SpawnPos[idx].forward * randDist;
            //생성
            var enemy = PhotonNetwork.Instantiate("Enemy", createpos, ranrot);
            enemy.GetComponent<Enemy>().Ac_onDie = OnDieEnemy;
        }
    }


    void OnDieEnemy()
    {
        currEnemy--;
        if (currEnemy <= 0)
        {
            currEnemy = 0;
        }
    }
}