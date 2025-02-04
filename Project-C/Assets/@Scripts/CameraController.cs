using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : InitBase
{
    public BaseObject Target { get; set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Camera.main.orthographicSize = 4.5f;
        return true;
    }

    void LateUpdate()
    {
        if (Target == null)
            return;

        Vector2 targetPos = Target.transform.position + new Vector3(Target.Collider.offset.x, Target.Collider.offset.y);
        transform.position = new Vector3(targetPos.x, targetPos.y, -10);
    }
}