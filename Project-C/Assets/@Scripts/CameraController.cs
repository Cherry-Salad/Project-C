using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{
    public CinemachineVirtualCamera VirtualCamera;
    public CinemachineConfiner2D Confiner;

    BaseObject _target;
    /// <summary>
    /// 카메라가 추적할 대상
    /// </summary>
    public BaseObject Target 
    {
        get { return _target; }
        set 
        { 
            _target = value;
            VirtualCamera.Follow = _target.transform;
            VirtualCamera.LookAt = _target.transform;
        }
    }

    //float _halfHeight;  // 카메라의 반 높이
    //float _halfWidth;   // 카메라의 반 너비

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //Camera.main.orthographicSize = 4.5f;

        //_halfHeight = Camera.main.orthographicSize;
        //_halfWidth = Camera.main.aspect * _halfHeight;

        return true;
    }

    void Start()
    {
        VirtualCamera = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        VirtualCamera.m_Lens.OrthographicSize = 4.5f;
        Confiner = VirtualCamera.transform.GetComponent<CinemachineConfiner2D>();
    }

    void LateUpdate()
    {
        //if (Target == null)
        //    return;

        //// 타겟 위치
        //Vector2 targetPos = Target.transform.position + new Vector3(Target.Collider.offset.x, Target.Collider.offset.y);

        //// 맵 경계
        //Vector2 minBound = Managers.Map.MinBound;
        //Vector2 maxBound = Managers.Map.MaxBound;

        //// 카메라 위치를 월드 경계로 제한한다
        //float clampedX = Mathf.Clamp(targetPos.x, minBound.x + _halfWidth, maxBound.x - _halfWidth);
        //float clampedY = Mathf.Clamp(targetPos.y, minBound.y + _halfHeight, maxBound.y - _halfHeight);

        //transform.position = new Vector3(targetPos.x, targetPos.y, -10);
    }
}