using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    SPAWN,
    IDLE,
    PATROL,
    MOVE,
    ATTACK,
    DIE,
}


public class Enemy : MonoBehaviourPun
{
    //현재 나의 상태 표현
    public EnemyState State;

    //현재 시간
    float CurrentTime;

    //Spawn 유지 시간
    public float spawnTime = 3;

    //대기 유지시간
    public float idleTime = 3;

    // Animator
    public Animator anim;

    //NavMeshAgent
    NavMeshAgent navi;

    //탐지거리
    public float searchDist = 10;
    //탐지시야
    public float searchAngle = 120;

    //공격 범위
    public float attackDist = 3;

    /// <summary>
    /// 공격 대기 시간
    /// </summary>
    public float attackDealyTime = 2;

    //타겟
    public Transform T_trTarget;

    //죽었을 때 호출해주는 함수를 담을 변수
    public System.Action Ac_onDie;

    [SerializeField] GameObject exploFactory;
    [SerializeField] Transform trBody;
    [SerializeField] Transform trEye;


    // Start is called before the first frame update
    void Start()
    {
        navi = GetComponent<NavMeshAgent>();
        navi.enabled = PhotonNetwork.IsMasterClient;
        searchAngle = Mathf.Cos(searchAngle * Mathf.Deg2Rad * 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        switch (State)
        {
            case EnemyState.SPAWN:
                UpdateSapwn();
                break;
            case EnemyState.IDLE:
                UpdateIdle();
                break;
            case EnemyState.MOVE:
                UpdateMove();
                break;
            case EnemyState.ATTACK:
                UpdateAttack();
                break;
            case EnemyState.DIE:
                UpdateDie();
                break;
            case EnemyState.PATROL:
                UpdatePatrol();
                break;
        }

        Vector3 v = trEye.forward;
        v.y = 0;

        Debug.DrawRay(trEye.position, Quaternion.Euler(0, -30, 0) * v * 100, Color.red);
        Debug.DrawRay(trEye.position, Quaternion.Euler(0, 30, 0) * v * 100, Color.red);

        if(T_trTarget != null)
        {
            Debug.DrawLine(trEye.position, T_trTarget.position, Color.blue);
        }

    }

    private void UpdatePatrol()
    {
        //플레이어 공격범위, 시야에 들어왔는지 체크
        if (Detect())
        {
            //상태를 MOVE로 바꾸자
            ChangeState(EnemyState.MOVE);
        }
        //도착했으면 Idle 상태로 전환
        if (navi.remainingDistance < 0.2f)
        {
            ChangeState(EnemyState.IDLE);
        }
    }

    private void UpdateDie()
    {
        CurrentTime += Time.deltaTime;
        if (CurrentTime > 1.5f)
        {
            //두번 불릴 수 있기 때문에 소멸 상태라는 걸 만들어두면 좋음;
            photonView.RPC(nameof(RpcDestory), RpcTarget.All);
        }
    }

    [PunRPC]
    void RpcDestory()
    {
        var explo = Instantiate(exploFactory, trBody.position, default);
        Destroy(explo, 3f);
        Destroy(gameObject);
    }


    /// <summary>
    /// 상태가 변화될때 한번만 호출하는 함수
    /// </summary>
    /// <param name="s"></param>
    public void ChangeState(EnemyState s)
    {   
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }


        //만약에 현재 상태가 DIE라면 함수를 나가자
        if (State == EnemyState.DIE)
        {
            return;
        }


        //navi 멈추자.
        navi.isStopped = true;
        //현재 시간 초기화
        CurrentTime = 0;

        //현재 내 상태 변경
        State = s;
        //애니메이션 변경
        photonView.RPC(nameof(RpcSetTrigger), RpcTarget.All, State.ToString());
        switch (s)
        {
            case EnemyState.SPAWN:
                break;
            case EnemyState.IDLE:
                break;
            case EnemyState.MOVE:
                navi.isStopped = false;
                break;
            case EnemyState.ATTACK:
                break;
            case EnemyState.DIE:
                Ac_onDie?.Invoke();
                break;
            case EnemyState.PATROL:
                navi.isStopped = false;
                StartCoroutine(GetRandomPos());
                break;
            default:
                break;
        }
    }


    IEnumerator GetRandomPos()
    {
        while (true)
        {
            var dist = Random.Range(5, 10f);
            var angle = Random.Range(0, 360f);

            //앞 방향을 angle 만큼 y축으로 회전 시킨 값
            var dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            //랜덤 위치
            var pos = transform.position + dir * dist;

            //pos 위치에서 위로 100만큼 떨어진 곳에서 Ray 쏘자

            Ray ray = new Ray(pos + Vector3.up * 100, Vector3.down);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
            {
                navi.SetDestination(hit.point);
                break;
            }
            yield return null;
        }
    }


    [PunRPC]
    void RpcSetTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }


    [PunRPC]
    void RpcDie()
    {
        ChangeState(EnemyState.DIE);
    }


    void UpdateSapwn()
    {
        CurrentTime += Time.deltaTime;
        //3초가 지나면 
        if (CurrentTime > spawnTime)
        {
            //상태를 IDLE로 변경
            ChangeState(EnemyState.IDLE);
        }
    }

    /// <summary>
    /// 거리별 정리
    /// </summary>
    int SortByDist(Collider m, Collider n)
    {
        var distM = Vector3.Distance(m.transform.position, transform.position);
        var distN = Vector3.Distance(m.transform.position, transform.position);
        if (distM > distN)
        {
            return 1;
        }


        return -1;
    }


    void UpdateIdle()
    {
        //시간을 흐르게 한다.
        CurrentTime += Time.deltaTime;

        if (CurrentTime > spawnTime)
        {
            //패트롤 상태로 변경
            ChangeState(EnemyState.PATROL);
        }

        if (Detect())
        {
            //상태를 MOVE로 바꾸자
            ChangeState(EnemyState.MOVE);
        }
    }

    bool Detect()
    {

        int playerLayer = 1 << LayerMask.NameToLayer("Player");
        //플레이어와의 거리가 탐지거리에 들어왔다면 
        var players = Physics.OverlapSphere(transform.position, searchDist, playerLayer);



        if (players.Length == 0) return false;

        //플레이어가 내 시야에 들어왔다면 둘중에 아무거나 쓰면 됨
        //var angle =  Vector3.Angle(transform.forward, players[0].transform.position - transform.position);
        //if (angle > 60) return;
        //print("시야에 들어옴11");

        //가까운 놈을 맨 앞으로 정렬
        var listplayers = players.ToList();
        listplayers.Sort(SortByDist);
        //listplayers.Sort((Collider m, Collider n) =>
        //{
        //    var distM = Vector3.Distance(m.transform.position, transform.position);
        //    var distN = Vector3.Distance(m.transform.position, transform.position);
        //    if (distM > distN)
        //    {
        //        return 1;
        //    }
        //    return -1;
        //});

        Vector3 v1 = trEye.forward;
        var dir = listplayers[0].transform.position - trEye.position;
        v1.y = 0;
        dir.y = 0;

        //v1,v2를 정규화 한 값으로 dot을 해야한다
        var dot = Vector3.Dot(v1.normalized, dir.normalized);
        if (dot < searchAngle) return false;
        print("시야에 들어옴22");

        //타겟 설정
        T_trTarget = listplayers[0].transform;

        #region 타겟의 앞 뒤 옆 오른쪽 판별
        //var angle = Vector3.Angle(transform.forward, dir);
        //if (angle < 90)
        //{
        //    print("앞에 있음");
        //}
        //else if (angle > 90)
        //{
        //    print("뒤에 있음");
        //}

        //angle = Vector3.Angle(transform.right, dir);
        //if (angle < 90)
        //{
        //    print("오른쪽에 있음");
        //}
        //else if (angle > 90)
        //{
        //    print("왼쪽에 있음");
        //}
        #endregion
        return true;
    }


    void UpdateMove()
    {
        //타겟의 위치를 목적지로 정하자.
        //높이가 맞지 않아 문제가 발생할 수 있음
        navi.SetDestination(T_trTarget.position);

        //만약에 타겟이 공격 거리에 있다면
        var dist = Vector3.Distance(T_trTarget.position, transform.position);

        if (dist < attackDist)
        {
            //공격 상태로 전환
            ChangeState(EnemyState.ATTACK);
        }
    }


    void UpdateAttack()
    {
        CurrentTime += Time.deltaTime;
        if(CurrentTime > attackDealyTime )
        {
            var dist = Vector3.Distance(T_trTarget.position, transform.position);

            if(dist < attackDist)
            {
                ChangeState(EnemyState.ATTACK);
            }
            else
            {
                ChangeState(EnemyState.IDLE);
            }
        }
    }
}
