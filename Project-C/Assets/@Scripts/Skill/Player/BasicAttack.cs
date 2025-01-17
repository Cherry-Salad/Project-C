using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BasicAttack : PlayerSkillBase
{
    Vector2 _skillDir { get { return (Owner.LookLeft) ? Vector2.left : Vector2.right; } }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(Creature owner, SkillData data)
    {
        base.SetInfo(owner, data);

        // 기본 공격은 스킬 데이터로 구성하지 않았다. 임시로 직접 구성하였다.
        Name = "BasicAttack";
        AnimationName = "BasicAttack";
        CastingTime = 0f;
        RecoveryTime = 0f;
        CoolTime = 1f;  // 임시 값
        DamageMultiplier = 1.0f;   // 임시 값
        AttackRange = 1f;   // 임시 값

        IsUnlock = true;
        Level = 0;
        Key = KeyCode.Z;
        MpCost = 0;
        MaxLevel = 1;  // 임시 값
    }

    public override bool IsSkillUsable()
    {
        if (base.IsSkillUsable() == false)
            return false;

        return true;
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false) 
            return false;

        Owner.Animator.Play(AnimationName);
        Owner.State = ECreatureState.Skill;
        StartCoroutine(CoDoSkill());

        // 사실 이펙트 프리팹을 사용하여 프리팹과 충돌한 애들에게 피격을 주고 싶다. 하지만, 기본 공격 이펙트는 애니메이션에 종속되어 있어 귀찮다.
        //Collider2D[] hitTarget = Physics2D.OverlapBoxAll(skillPos, boxSize, 0f);

        //foreach (Collider2D target in hitTarget)
        //{
        //    MonsterBase monster = target.GetComponent<MonsterBase>();
        //    if (monster != null)
        //    {
        //        // TODO: 피격 판정
        //    }
        //}

        return true;
    }

    IEnumerator CoDoSkill()
    {
        AnimatorStateInfo stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(AnimationName) == false)
        {
            // 스킬 애니메이션 재생 대기 중
            yield return null;
            stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
        }

        float duration = stateInfo.length + RecoveryTime;   // (애니메이션 길이) + (후 딜레이)
        float elapsedTime = 0f;

        // 스킬 애니메이션 재생 중
        while (elapsedTime < duration && stateInfo.IsName(AnimationName))
        {
            // 피격 시 스킬 취소
            if (Owner.State == ECreatureState.Hurt)
                yield break;

            stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
            elapsedTime = duration * stateInfo.normalizedTime;
            Debug.Log($"duration: {duration:F2}, elapsedTime: {elapsedTime:F2}");
            yield return null;
        }

        Debug.Log("EndSkill");
        Owner.State = ECreatureState.Idle;
    }

    void OnDrawGizmos() // 디버깅을 위하여 스킬 범위 시각화
    {
        if (Owner == null)
            return;

        // 가운데 위치 조정
        Vector2 boxCenter = Owner.Rigidbody.position + (_skillDir * 3f * Owner.Collider.bounds.extents.x);
        boxCenter.y += 0.1f;
        
        // 크기
        Vector2 boxSize = new Vector2(AttackRange, 1f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
