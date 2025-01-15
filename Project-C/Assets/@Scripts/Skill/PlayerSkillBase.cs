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

    protected int _mpCost = 0;
    protected int _maxLevel = 0;

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
        Key = (Enum.TryParse(PlayerSkillData.DefaultBoundKey, out KeyCode key)) ? key : KeyCode.None;
        _mpCost = PlayerSkillData.MPCost;
        _maxLevel = PlayerSkillData.MAXLevel;
    }

    public override bool IsSkillUsable()
    {
        if (base.IsSkillUsable() == false) 
            return false;

        if (IsUnlock == false || Owner.Mp < _mpCost)
            return false;

        return true;
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false) 
            return false;

        Owner.Mp -= _mpCost;
        return true;
    }
}
