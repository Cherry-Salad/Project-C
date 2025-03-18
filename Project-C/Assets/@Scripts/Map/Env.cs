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

    public void SetInfo(int dataId, Vector3 pos, bool flipX = false, bool flipY = false)
    {
        Data = Managers.Data.EnvDataDic[dataId];
        Hp = Data.Hp;
        transform.position = pos;

        if (flipX)
        {
            //Debug.Log("flipX");
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        
        if (flipY)
        {
            //Debug.Log("flipY");
            Vector3 localScale = transform.localScale;
            localScale.y *= -1;
            transform.localScale = localScale;
        }
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
            {
                Hp--;
                StartCoroutine(CoShake());
            }
        }
    }

    protected IEnumerator CoShake(float duration = 0.2f)
    {
        Vector3 pos = transform.position;
        float magnitude = 0.05f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            transform.position = pos + new Vector3(x, 0, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = pos; // 원래 위치로 복구

        if (Hp <= 0)
            Managers.Resource.Destroy(gameObject);  // TODO: 부숴지는 연출
    }
}
