using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : PlayerSkillBase
{
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
        RecoveryTime = 1f; // 임시 값
        DamageMultiplier = 1.0f;   // 임시 값
        AttackRange = 1.5f;    // 임시 값

        IsUnlock = true;
        Level = 0;
        Key = KeyCode.Z;
        MpCost = 0;
        MaxLevel = 1;  // 임시 값
    }

    public override bool IsSkillUsable()
    {
        return base.IsSkillUsable();
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false) 
            return false;

        Debug.Log($"DoSkill: BasicAttack");
        // TODO: 피격 판정
        return true;
    }
}
