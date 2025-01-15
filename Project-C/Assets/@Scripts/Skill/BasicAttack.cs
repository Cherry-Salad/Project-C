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
        _type = "Melee";
        _castTime = 0f;
        _recoveryTime = 1f; // 임시 값
        _damageMultiplier = 1.0f;   // 임시 값
        _attackRange = 1.5f;    // 임시 값

        IsUnlock = true;
        Level = 0;
        Key = KeyCode.Z;
        _mpCost = 0;
        _maxLevel = 1;  // 임시 값
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
