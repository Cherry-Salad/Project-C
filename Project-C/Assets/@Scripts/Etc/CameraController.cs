using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{
    public CinemachineVirtualCamera VirtualCamera { get; private set; }
    public CinemachineConfiner2D Confiner { get; private set; }

    BaseObject _target;
    /// <summary>
    /// 카메라가 추적할 대상
    /// </summary>
    public BaseObject Target 
    {
        get { return _target; }
        set 
        {
            if (_target != value)
            {
                _target = value;
                VirtualCamera.Follow = _target.transform;
                VirtualCamera.LookAt = _target.transform;
            }
        }
    }

    PolygonCollider2D _boundary;
    public PolygonCollider2D Boundary 
    { 
        get
        {
            if (Confiner == null || Confiner.m_BoundingShape2D == null)
                return null;
            return _boundary;
        }
        private set
        {
            if (_boundary != value)
            {
                _boundary = value;
                Confiner.m_BoundingShape2D = _boundary;
            }
        }
    }

    int _priority;
    public int Priority
    {
        get { return _priority; }
        set
        {
            if (_priority != value)
            {
                _priority = value;
                VirtualCamera.Priority = _priority;
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        VirtualCamera = GetComponent<CinemachineVirtualCamera>();
        Confiner = GetComponent<CinemachineConfiner2D>();
        VirtualCamera.m_Lens.OrthographicSize = 5.625f; // (180 / 16) * 0.5 = 5.625
        return true;
    }

    public void SetInfo(BaseObject target, PolygonCollider2D boundary, int priority = 10)
    {
        //VirtualCamera = GetComponent<CinemachineVirtualCamera>();
        //Confiner = GetComponent<CinemachineConfiner2D>();

        Target = target;
        Boundary = boundary;
        Priority = priority;
    }
}