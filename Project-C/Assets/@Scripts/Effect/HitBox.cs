using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : InitBase
{
    public Creature Owner { get; private set; }
    public SkillBase Skill { get; private set; }
    public BoxCollider2D Collider { get; private set; }

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
                Vector3 localPos = transform.localPosition;
                localPos.x *= -1;
                transform.localPosition = localPos;

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

    public void SetInfo(Creature owner, SkillBase skill, bool lookLeft, LayerMask excludeLayers, bool setActive = true)
    {
        Owner = owner;
        Skill = skill;
        LookLeft = lookLeft;
        Collider.excludeLayers = excludeLayers;
        gameObject.SetActive(setActive);
    }

    public void OnDestroy()
    {
        Managers.Resource.Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"{collision.gameObject.name}");

        // SetInfo에서 충돌 대상을 다 필터링해서, BaseObject만 찾으면 된다.
        // 하지만, 몬스터는 Hit 함수가 따로 있어서 일단 구분하였다.
        BaseObject target = collision.GetComponent<BaseObject>();
        if (target != null)
        {
            float damage = Owner.Atk * Skill.DamageMultiplier;  // 오너 공격력 * 스킬 공격력
            MonsterBase monster = target as MonsterBase;

            if (monster != null)
            {
                // 버그 확인 필요: 아주 간혹, 몬스터의 BodyHitBox가 비활성되어 피격 처리가 안된다.
                // Test를 위하여 공격력을 낮췄으니 직접 테스트 해보길 바랍니다. 
                //monster.Hit((int)damage);
                monster.Hit();  // Test
            }
            else
                target.OnDamaged(damage, Owner);
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
