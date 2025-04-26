using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxMpUpItem : Env
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;
        return true;
    }

    public override void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null) {}

    public override void OnPickedUp()
    {
        base.OnPickedUp();
        Managers.Game.Player.MaxMp++;
        OnDied();
    }

    public override void OnDied()
    {
        base.OnDied();

        Managers.Map.DespawnObject(this);
        Managers.Resource.Destroy(gameObject);  // For 최혁도, TODO: 부숴지는 연출
    }
}
