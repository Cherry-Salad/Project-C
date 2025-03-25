using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : InitBase
{
    public BaseScene Scene { get; private set; }

    public override bool Init()
    {
        return base.Init();
    }

    public void SetInfo(Vector3 pos, BaseScene scene)
    {
        transform.position = pos;
        Scene = scene;
    }
}
