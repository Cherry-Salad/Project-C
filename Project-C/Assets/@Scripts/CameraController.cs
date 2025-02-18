using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{
    public CinemachineVirtualCamera VirtualCamera { get; set; }
    public CinemachineConfiner2D Confiner { get; set; }

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
            if (Confiner.m_BoundingShape2D == null)
                return null;
            return _boundary;
        }
        set
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
        get
        {
            if (VirtualCamera == null)
                return -1;
            else
                return _priority;
        }
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
        return true;
    }

    public void SetInfo(BaseObject target, PolygonCollider2D boundary, int priority = 10)
    {
        //VirtualCamera = GetComponent<CinemachineVirtualCamera>();
        //Confiner = GetComponent<CinemachineConfiner2D>();

        Target = target;
        Boundary = boundary;
        Priority = priority;

        VirtualCamera.m_Lens.OrthographicSize = 4.5f;
    }
}