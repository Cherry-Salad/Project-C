using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SavePoint : InitBase
{
    public BoxCollider2D Collider { get; private set; }
    public BaseScene Scene { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = GetComponent<BoxCollider2D>();

        // 필터링
        LayerMask includeLayers = 0;
        includeLayers.AddLayer(ELayer.Player);
        Collider.includeLayers = includeLayers;

        return true;
    }

    public void SetInfo(Vector3 pos, BaseScene scene)
    {
        transform.position = pos;
        Scene = scene;
    }
}
