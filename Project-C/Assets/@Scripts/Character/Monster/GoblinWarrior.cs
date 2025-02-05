using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class GoblinWarrior : MonsterBase
{
    private const int _HITBOX_NUM_BODY = 0;
    private const int _HITBOX_NUM_SWORD_ATTACK = 1;

    private const int _SWORD_ATTACK_SKILL_NUMBER = 0;

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
        skillList.Add(new Tuple<int, IEnumerator>(_SWORD_ATTACK_SKILL_NUMBER, SwordAttack()));

        shufflingSkill(skillList);
    }

    IEnumerator SwordAttack()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_SWORD_ATTACK_SKILL_NUMBER];

        float posX = this.transform.position.x + (MoveDir.x * skillData.HitBoxPos);
        hitBoxList[_HITBOX_NUM_SWORD_ATTACK].transform.position = new Vector2(posX, this.transform.position.y);
        
        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void SwordAttackActiveHitBox()
    {
        ActiveHitBox(_HITBOX_NUM_SWORD_ATTACK);
    }
}
