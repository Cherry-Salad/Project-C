using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScene : InitBase
{
    public Define.EScene SceneType { get; protected set; } = Define.EScene.None;

    public override bool Init()
    {
        return base.Init();
    }

    public virtual void Clear() {}
}
