using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackHitBox : InitBase
{
    BoxCollider2D Collider;
    public float DamageMultiplier { get; set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = GetComponent<BoxCollider2D>();
        return true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        MonsterBase monster = collision.GetComponent<MonsterBase>();
        if (monster != null)
        {
            monster.OnDamaged(DamageMultiplier);
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
