using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BasicAttack : PlayerSkillBase
{
    Vector2 _skillDir { get { return (Owner.LookLeft) ? Vector2.left : Vector2.right; } }
    GameObject _hitBox;

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
        CoolTime = 0f;  // 임시 값
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

        Owner.Mp -= MpCost;

        Owner.Animator.Play(AnimationName);
        Owner.State = ECreatureState.Skill;
        StartCoroutine(CoDoSkill());

        return true;
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출하며, 히트 박스를 생성한다.
    /// </summary>
    void SpawnHitBox()
    {
        //Vector2 skillPos = Owner.Rigidbody.position + (_skillDir * 3f * Owner.Collider.bounds.extents.x);
        //skillPos.y += 0.1f;
        //Vector2 hitBoxSize = new Vector2(AttackRange, 1f);
        //Collider2D[] hitTarget = Physics2D.OverlapBoxAll(skillPos, hitBoxSize, 0f);

        //foreach (Collider2D target in hitTarget)
        //{
        //    MonsterBase monster = target.GetComponent<MonsterBase>();
        //    if (monster != null)
        //        monster.OnDamaged(DamageMultiplier);
        //}

        _hitBox = Managers.Resource.Instantiate("BasicAttackHitBox", transform);
        _hitBox.GetComponent<BasicAttackHitBox>().DamageMultiplier = DamageMultiplier;

        if (_skillDir.x < 0)
        {
            // 스킬 방향이 왼쪽이라면 x축 반전
            Vector3 localScale = _hitBox.transform.localScale;
            localScale.x *= -1;
            _hitBox.transform.localScale = localScale;
        }
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출하며, 생성한 히트 박스를 없앤다.
    /// </summary>
    void DespawnHitBox()
    {
        // 커밋 연습 테스트
        if (_hitBox != null)
            Managers.Resource.Destroy(_hitBox);
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
        while (elapsedTime < duration)
        {
            // 피격 시 스킬 취소
            if (Owner.State == ECreatureState.Hurt || stateInfo.IsName(AnimationName) == false)
            {
                DespawnHitBox();
                yield break;
            }

            stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
            elapsedTime = duration * stateInfo.normalizedTime;
            //Debug.Log($"duration: {duration:F2}, elapsedTime: {elapsedTime:F2}");
            yield return null;
        }

        //Debug.Log("EndSkill");
        // 캐릭터가 공중에 있으면 점프로 전환
        Owner.State = Owner.CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }
}
