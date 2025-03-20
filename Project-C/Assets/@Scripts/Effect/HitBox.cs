using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : InitBase
{
    public Creature Owner { get; private set; }
    public SkillBase Skill { get; private set; }
    public Rigidbody2D Rigidbody { get; protected set; }    // 트리거 이벤트를 처리하기 위해서는 부모가 Rigidbody가 있거나 본인의 Rigidbody가 반드시 필요
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

        Rigidbody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();

        Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;  // Z축 고정
        return true;
    }

    public void SetInfo(Vector3 spawnPos, Creature owner, SkillBase skill, bool lookLeft, LayerMask excludeLayers)
    {
        transform.localPosition = spawnPos;
        Owner = owner;
        Skill = skill;
        LookLeft = lookLeft;
        Collider.excludeLayers = excludeLayers;
        gameObject.SetActive(true);
        //if (setActive)
        //{
        //    gameObject.SetActive(true);
        //
        //    // 히트 박스가 비활성화 되어도 Collider 상태를 기억하고 있다.
        //    // 그래서 다시 활성화할 때 새로운 충돌로 인식하도록 강제로 충돌 감지 초기화
        //    //Collider.enabled = false;
        //    //Collider.enabled = true;
        //}
    }

    public void OnDestroy()
    {
        Managers.Resource.Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"{collision.name}");

        // SetInfo에서 충돌 대상을 다 필터링해서, BaseObject만 찾으면 된다.
        // 하지만, 몬스터는 Hit 함수가 따로 있어서 일단 구분하였다.
        BaseObject target = collision.GetComponent<BaseObject>();
        if (target != null)
        {
            float damage = Owner.Atk * Skill.DamageMultiplier;  // 오너 공격력 * 스킬 공격력
            MonsterBase monster = target as MonsterBase;

            if (monster != null)
                monster.Hit((int)damage);
            else
                target.OnDamaged(damage, attacker: Owner.Collider);
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
