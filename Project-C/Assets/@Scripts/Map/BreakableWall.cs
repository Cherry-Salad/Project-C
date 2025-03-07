using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env : BaseObject
{
    public Data.EnvData Data;
    public float Hp { get; set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Env;
        return true;
    }

    public void SetInfo(int dataId, Vector3 pos, Sprite sprite)
    {
        Data = Managers.Data.EnvDataDic[dataId];
        Hp = Data.Hp;
        transform.position = pos;
        SpriteRenderer.sprite = sprite;
    }

    public override void OnDamaged(float damage, bool ignoreInvincibility = false, Collider2D attacker = null)
    {
        base.OnDamaged(damage, ignoreInvincibility, attacker);

        if (attacker != null)
        {
            BaseObject obj = attacker.GetComponent<BaseObject>();
            if (obj == null)
                return;

            if (obj.ObjectType == Define.EObjectType.Player)
                Hp--;
            
            if (Hp <= 0)
                Managers.Resource.Destroy(gameObject);
        }
    }
}
