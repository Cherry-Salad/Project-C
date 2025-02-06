using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static void AddLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask |= (1 << (int)layer);
    }
}
