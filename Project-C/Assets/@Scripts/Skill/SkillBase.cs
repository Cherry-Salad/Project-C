using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase : InitBase
{
    public Creature Owner { get; private set; }

    public SkillData Data;

    protected string _type; // 스킬 유형
    protected string _usageCondition;   // 사용 조건

    protected float _castTime;  // 시전 시간
    protected float _recoveryTime;  // 시전 후 회복 시간
    protected float _damageMultiplier;  // 데미지 배율
    protected float _attackRange;   // 공격 범위

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

        _type = Data.Type;
        _usageCondition = Data.UsageCondition;
        _castTime = Data.Effect.CastTime[0];    // TODO: 수정 필요
        _recoveryTime = Data.Effect.RecoveryTime;
        _damageMultiplier = Data.Effect.DamageMultiplier;
        _attackRange = Data.Effect.AttackRange;
    }

    public virtual bool IsSkillUsable()
    {
        return true;
    }

    public virtual bool DoSkill()
    {
        if (IsSkillUsable() == false) 
            return false;

        return true;
    }
}