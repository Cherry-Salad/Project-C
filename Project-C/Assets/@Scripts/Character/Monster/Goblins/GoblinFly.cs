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
    private bool _isAttack;
    private float _ATTACK_TIME = 0.5f;
    private float _ATTACK_READY_SPEED = 0.5f;

    protected override void UpdateAnimation()
    {
        if(State != ECreatureState.Skill)
        {
            base.UpdateAnimation();
            return;
        }

        switch (State)
        {
            case ECreatureState.Skill:  
                if(_isAttack) 
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
        Vector2 attackDir = getDirection(this.transform.position, TargetGameObject.transform.position);

        Rigidbody.velocity = attackDir * -1;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterFly_BodySlam_Ready); //고블린 플라이 BodySlam Ready SFX 재생
        yield return new WaitForSeconds(_ATTACK_READY_SPEED);

        _isAttack = true;
        UpdateAnimation();
        Rigidbody.velocity = attackDir * TypeRecorder.Battle.Attack[_BODY_SLAM_SKILL_NUMBER].MovementMultiplier;
        ActiveHitBox(_HITBOX_NUM_BODY);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterFly_BodySlam_Shot); //고블린 플라이 BodySlam Shot SFX 재생

        yield return new WaitForSeconds(_ATTACK_TIME);
        _isAttack = false;
        DeactivateHitBox();

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }
}
