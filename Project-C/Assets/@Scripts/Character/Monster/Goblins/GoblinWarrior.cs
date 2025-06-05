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
    private bool _isReady = true;

    protected override void UpdateAnimation()
    {
        if (State != ECreatureState.Skill)
            base.UpdateAnimation();
        
        switch (State)
        {            
            case ECreatureState.Skill:
                if (_isReady)
                    Animator.Play("AttackReady");
                else
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

        _isReady = true;
        UpdateAnimation();

        yield return new WaitForSeconds(skillData.WindUpTime);

        _isReady = false;
        UpdateAnimation();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterWarrior); //고블린 워리어 기본공격 SFX
        float posX = this.transform.position.x + (MoveDir.x * skillData.HitBoxPos);
        hitBoxList[_HITBOX_NUM_SWORD_ATTACK].transform.position = new Vector2(posX, this.transform.position.y);
        
        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void SwordAttackActiveHitBox()
    {
        ActiveHitBox(_HITBOX_NUM_SWORD_ATTACK);
    }
}
