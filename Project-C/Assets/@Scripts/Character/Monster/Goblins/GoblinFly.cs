using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class GoblinFly : FlyMonsterBase
{
    private const int _HITBOX_NUM_BODY = 0;

    private const int _BODY_SLAM_SKILL_NUMBER = 0;

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        switch (State)
        {
            case ECreatureState.Skill:
                Animator.Play("Attack");
                break;
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        RegistrationSkill();
        DeactivateHitBox();

        return true;
    }

    protected override void RegistrationSkill()
    {
        skillList.Clear();
        skillList.Add(new Tuple<int, IEnumerator>(_BODY_SLAM_SKILL_NUMBER, BodySlam()));

        shufflingSkill(skillList);
    }

    IEnumerator BodySlam()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_BODY_SLAM_SKILL_NUMBER];
        
        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void SwordAttackActiveHitBox()
    {
        ActiveHitBox(_HITBOX_NUM_BODY);
    }
}
