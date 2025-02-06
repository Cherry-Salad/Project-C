using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GoblinBoomerangThrower : MonsterBase
{
    private const int _HITBOX_NUM_BODY = 0;

    private const int _THROW_BOOMERANG_SKILL_NUMBER = 0;

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
        skillList.Add(new Tuple<int, IEnumerator>(_THROW_BOOMERANG_SKILL_NUMBER, ThrowBoomerang()));

        shufflingSkill(skillList);
    }

    IEnumerator ThrowBoomerang()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_THROW_BOOMERANG_SKILL_NUMBER];

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }
}
