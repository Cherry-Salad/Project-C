using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackHitBox : InitBase
{
    BoxCollider2D Collider;
    float _damageMultiplier;

    bool _lookLeft;
    public bool LookLeft
    {
        get { return _lookLeft; }
        set 
        {
            if (LookLeft != value)
            {
                _lookLeft = value;
                
                // 히트 박스 x축 반전
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = GetComponent<BoxCollider2D>();
        return true;
    }

    public void SetInfo(bool lookLeft, float damageMultiplier, bool setActive = true)
    {
        LookLeft = lookLeft;
        _damageMultiplier = damageMultiplier;
        gameObject.SetActive(setActive);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        MonsterBase monster = collision.GetComponent<MonsterBase>();
        if (monster != null)
        {
            monster.OnDamaged(_damageMultiplier);
        }
    }

    void OnDrawGizmos()
    {
        if (Collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Collider.bounds.center, Collider.bounds.size);
        }
    }
}
