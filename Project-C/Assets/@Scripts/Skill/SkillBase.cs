using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase : InitBase
{
    public Creature Owner { get; private set; }

    public SkillData Data;

    public string Name { get; protected set; }
    public string AnimationName { get; protected set; }
    public float CastingTime { get; protected set; }    // 시전 시간
    public float RecoveryTime { get; protected set; }   // 시전 후 회복 시간
    public float DamageMultiplier { get; protected set; }   // 데미지 배율
    public float AttackRange { get; protected set; }    // 공격 범위

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
        CastingTime = Data.CastingTime;
        RecoveryTime = Data.RecoveryTime;
        DamageMultiplier = Data.DamageMultiplier;
        AttackRange = Data.AttackRange;
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