using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAniEvent : MonoBehaviour
{
    public void OnAttack()
    {
        var enemy = transform.root.GetComponent<Enemy>();
        enemy .T_trTarget.GetComponent<PlayerMove>().UpdateHp(-5);
    }
}
