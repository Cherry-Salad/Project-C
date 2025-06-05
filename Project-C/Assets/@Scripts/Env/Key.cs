using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.Examples.TMP_ExampleScript_01;

public class Key : Env
{
    [SerializeField] private string LockObjectName;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;
        return true;
    }

    public override void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null) { }

    public override void OnPickedUp()
    {
        base.OnPickedUp();
        GameObject lockChain = GameObject.Find(LockObjectName);

        if (lockChain == null)
        {
            Debug.LogWarning("LockChain 오브젝트를 찾을 수 없습니다.");
            return;
        }

        LockMapObject lockMap = lockChain.GetComponent<LockMapObject>();

        if (lockMap == null)
        {
            Debug.LogWarning("LockChain에 LockMapObject 컴포넌트가 없습니다.");
            return;
        }

        lockMap.OnDied();
        OnDied();
    }

    public override void OnDied()
    {
        base.OnDied();

        Managers.Map.DespawnObject(this);
        Managers.Resource.Destroy(gameObject);  // For 최혁도, TODO: 부숴지는 연출
    }
}
