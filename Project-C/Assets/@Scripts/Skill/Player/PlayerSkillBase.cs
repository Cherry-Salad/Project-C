using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillBase : SkillBase
{
    public PlayerSkillData PlayerSkillData { get; protected set; }

    public bool IsUnlock { get; set; }
    public int Level { get; set; }
    public KeyCode Key { get; set; }
    public float KeyPressedTime { get; set; }
    public int MpCost { get; set; }
    public int MaxLevel { get; set; }

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
        KeyPressedTime = PlayerSkillData.Dynamics.KeyPressedTime;
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

        if (!ConsumeMp(MpCost)) //마나 부족시 false
            return false;

        return true;
    }

    public bool ConsumeMp(int MpCost)
    {
        Player OwnerisPlayer = Owner as Player; //Owner가 player일 때(캐스팅)
        
        if(OwnerisPlayer == null)
        {
            return false;
        }

        if(OwnerisPlayer.Mp < MpCost)
        {
            Debug.Log($"마나가 부족합니다. 현재 마나 : {OwnerisPlayer.Mp}, 필요 마나 : {MpCost}");
        }

        OwnerisPlayer.Mp -= MpCost;
        OwnerisPlayer.TriggerOnMpChanged(); //마나 UI 업데이트 이벤트 호출
        return true;
    }
}
