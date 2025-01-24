using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillBase : InitBase
{
    public Creature Owner { get; private set; }
    public SkillData Data;

    public string Name { get; protected set; }
    public string AnimationName { get; protected set; }
    public int ProjectileId { get; protected set; }
    public float CastingTime { get; protected set; }    // 시전 시간
    public float RecoveryTime { get; protected set; }   // 후 딜레이
    public float CoolTime { get; protected set; }   // 시전 후 회복 시간
    public float HealingValue { get; protected set; }   // 회복량
    public float DamageMultiplier { get; protected set; }   // 데미지 배율
    public float AttackRange { get; protected set; }    // 공격 범위

    protected bool _isCooldownComplete = true;  // 쿨타임 완료 여부

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public virtual void SetInfo(Creature owner, SkillData data)
    {
        Owner = owner;
        Data = data;

        if (data == null)
            return;

        Name = Data.CodeName;
        AnimationName = Data.AnimationName;
        ProjectileId = Data.ProjectileId;
        CastingTime = Data.CastingTime;
        RecoveryTime = Data.RecoveryTime;
        CoolTime = Data.CoolTime;
        HealingValue = Data.HealingValue;
        DamageMultiplier = Data.DamageMultiplier;
        AttackRange = Data.AttackRange;
    }

    public virtual bool IsSkillUsable()
    {
        if (_isCooldownComplete == false)
            return false;

        return true;
    }

    public virtual bool DoSkill()
    {
        if (IsSkillUsable() == false) 
            return false;

        Debug.Log($"DoSkill: {Name}");
        return true;
    }

    /// <summary>
    /// 캐스팅을 시작한다. 피격 당하면 캐스팅을 취소한다.
    /// </summary>
    /// <param name="action">캐스팅을 완료하면 호출할 이벤트</param>
    /// <returns></returns>
    protected IEnumerator CoDoCastingSkill(Action action)
    {
        float elapsedTime = 0f;
        while (elapsedTime < CastingTime)
        {
            // 피격 받으면 캐스팅 취소
            if (Owner.State == ECreatureState.Hurt)
                yield break;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        action();
    }

    protected IEnumerator CoSkillCooldown()
    {
        _isCooldownComplete = false;
        yield return new WaitForSeconds(CoolTime);
        _isCooldownComplete = true;
    }
}