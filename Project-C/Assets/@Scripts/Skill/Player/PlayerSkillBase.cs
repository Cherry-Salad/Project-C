using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillBase : SkillBase
{
    public PlayerSkillData PlayerSkillData;

    public bool IsUnlock { get; set; }
    public int Level { get; set; }
    public KeyCode Key { get; set; }
    public int MpCost = 0;
    public int MaxLevel = 0;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(Creature owner, SkillData data)
    {
        base.SetInfo(owner, data);
        PlayerSkillData = data as PlayerSkillData;

        if (PlayerSkillData == null)
            return;

        IsUnlock = PlayerSkillData.Dynamics.IsUnlock;
        Level = PlayerSkillData.Dynamics.Level;
        Key = PlayerSkillData.Dynamics.Key;
        MpCost = PlayerSkillData.MPCost;
        MaxLevel = PlayerSkillData.MAXLevel;
    }

    public override bool IsSkillUsable()
    {
        if (base.IsSkillUsable() == false)
            return false;

        // 스킬이 잠금되어 있거나 마나가 부족하면 사용 불가능
        if (IsUnlock == false || Owner.Mp < MpCost)
            return false;

        return true;
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false)
            return false;

        return true;
    }
}
