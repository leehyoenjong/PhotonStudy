using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class EnemyBody : MonoBehaviour,IDamaged
{
    public void OnDamaged(int damage)
    {
        //transform.root 최상위 부모
        transform.root.GetComponent<PhotonView>().RPC("RpcDie", RpcTarget.MasterClient);

    }
}
