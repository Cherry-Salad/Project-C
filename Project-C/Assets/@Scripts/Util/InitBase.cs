using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBase : MonoBehaviour
{
    protected bool _init = false;   // 초기화 여부

    public virtual bool Init()
    {
        // 초기화 중복 방지
        if (_init)
            return false;

        _init = true;
        return true;
    }

    void Awake()
    {
        Init();
    }
}
