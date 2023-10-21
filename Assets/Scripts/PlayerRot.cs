using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRot : MonoBehaviourPun
{
    //회전 값 누적
    float rotX = 0;
    float rotY = 0;

    [Header("회전 속력")]
    [SerializeField] float rotSpped;

    [Header("카메라")]
    [SerializeField] GameObject M_Camera;

    //총을 아무도 못쏨
    //인원이 다 차면 방장부터 순서대로 공격 -> 그담사람 -> 그담

    // Start is called before the first frame update
    void Start()
    {
        //만약 내것이 아니라면 비활성화
        //카메라도 비활성화
        this.enabled = photonView.IsMine;
        M_Camera.SetActive(photonView.IsMine);

        rotX = transform.localEulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        //만약에 마우스가 활성화 되어 있따면 나가자
        if (Cursor.visible)
            return;

        //마우스 움직임 받는다.
        var mx = Input.GetAxis("Mouse X");
        var my = Input.GetAxis("Mouse Y");

        //마우스의 움직임 값으로 회전값을 누적
        rotX += mx * rotSpped * Time.deltaTime;
        rotY += my * rotSpped * Time.deltaTime;

        //누적된 회전값으로 설정하자.
        transform.localEulerAngles = new Vector3(0, rotX, 0);
        //카메라의 x 각도
        M_Camera.transform.localEulerAngles = new Vector3(-rotY, 0, 0);
    }
}