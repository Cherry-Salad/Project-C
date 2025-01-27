using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class IceBall : PlayerSkillBase
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
    }

    public override bool IsSkillUsable()
    {
        return base.IsSkillUsable();
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false) 
            return false;

        Debug.Log("DoSkill");
        
        Owner.Animator.Play(AnimationName);
        Owner.State = ECreatureState.Skill;
        //Owner.Mp -= MpCost;   // 테스트를 위하여 마나 소비는 껐다
        return true;
    }

    void OnSpawnIceBall()
    {
        SpawnProjectile(Owner.transform.position + (Vector3)_skillDir, (Vector3)_skillDir);
    }

    void OnEndSkill()
    {
        EndSkill();
    }
}
