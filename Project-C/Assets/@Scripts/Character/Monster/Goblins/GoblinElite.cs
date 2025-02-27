using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Define;

public class GoblinElite : BossMonsterBase
{
    private const int _HITBOX_NUM_BODY = 0;
    private const int _HITBOX_NUM_TORNADO = 1;

    private const int _TORNADO_SKILL_NUMBER = 0;
    private const int _THROWING_AXE_SKILL_NUMBER = 1;
    private const int _EYE_LASER_ATTACK_SKILL_NUMBER = 2;
    
    private const float TORNADO_SPAWN_POINT_X = 0.3f;

    private const int _MAX_AXE_COUNT = 20;
    private const int _MAX_LASER_COUNT = 20;

    private float[] _THROW_AXES_POINT_X = {0.5f, 1f, 1.5f, 2.0f, 2.5f};
    private float _THROW_AXES_HIGHT_MAX = 2f;
    private float _SHOOTING_AXE_CORRECTION = 0.25f;
    private int _shootingAxeCount;

    private bool _isSkillEnd;

    private Queue<MonsterProjectile> _axes = new Queue<MonsterProjectile>();
    private Queue<MonsterProjectile> _lasers = new Queue<MonsterProjectile>();

    protected override void UpdateAnimation()
    {
        if (State != ECreatureState.Skill)
            base.UpdateAnimation();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        RegistrationSkill();
        DeactivateHitBox();

        return true;
    }

    protected override void SettingProjectile()
    {
        
        for (int i = 0; i < _MAX_AXE_COUNT; i++)
            _axes.Enqueue(MakeProjectile(_THROWING_AXE_SKILL_NUMBER));
        
        for (int i = 0; i < _MAX_LASER_COUNT; i++)
            _lasers.Enqueue(MakeProjectile(_EYE_LASER_ATTACK_SKILL_NUMBER));
            
    }

    protected override void RegistrationSkill()
    {
        skillList.Clear();

        skillList.Add(new Tuple<int, IEnumerator>(_TORNADO_SKILL_NUMBER, GoblinTornado()));
        skillList.Add(new Tuple<int, IEnumerator>(_THROWING_AXE_SKILL_NUMBER, ThrowingAxe()));
        skillList.Add(new Tuple<int, IEnumerator>(_EYE_LASER_ATTACK_SKILL_NUMBER, EyeLaserAttack()));

        shufflingSkill(skillList);
    }

    IEnumerator GoblinTornado()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_TORNADO_SKILL_NUMBER];
        Animator.Play("GoblinTornadoStart");
            
        yield return new WaitForSeconds(skillData.RetentionTime);

        _isSkillEnd = true;
        isInvincibility = false;
        
        while(_isSkillEnd) yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    private IEnumerator GoblinTornadoMove()
    {
        try{
            tornadoHitBoxPosSetting();

            while (true)
            {
                if (_isSkillEnd && CheckGround()) break;

                if (CheckWall())
                {
                    TurnObject();
                    tornadoHitBoxPosSetting();

                    Rigidbody.velocity = new Vector2(MoveDir.x * TypeRecorder.Battle.Attack[_TORNADO_SKILL_NUMBER].MovementMultiplier, 7f);
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    Rigidbody.velocity = new Vector2(MoveDir.x * TypeRecorder.Battle.Attack[_TORNADO_SKILL_NUMBER].MovementMultiplier, Rigidbody.velocity.y);
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        finally{
            Animator.Play("Idle");
            DeactivateHitBox();
            SimpleStopMove();
            _isSkillEnd = false;
        }
    }

    private void tornadoHitBoxPosSetting()
    {
        if (LookLeft)
            hitBoxList[_HITBOX_NUM_TORNADO].transform.position = new Vector2(transform.position.x - 0.3f, hitBoxList[_HITBOX_NUM_TORNADO].transform.position.y);
        else
            hitBoxList[_HITBOX_NUM_TORNADO].transform.position = new Vector2(transform.position.x + 0.3f, hitBoxList[_HITBOX_NUM_TORNADO].transform.position.y);
    }

    public void ActiveHitBoxGoblinTornado()
    {
        _isSkillEnd = false;
        isInvincibility = true;

        ActiveHitBox(_HITBOX_NUM_TORNADO);
        StartCoroutine(GoblinTornadoMove());
    }

    IEnumerator ThrowingAxe()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_THROWING_AXE_SKILL_NUMBER];

        for(_shootingAxeCount = 0; _shootingAxeCount < skillData.NumberOfShots; _shootingAxeCount++)
        {
            ViewTarget();
            Animator.Play("ThrowingAxe");
            
            yield return new WaitForSeconds(skillData.DelayBetweenShots);
        }

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void ShootingAxe()
    {
        float dir = LookLeft ? -1f : 1f;
        float correction = _shootingAxeCount % 2 == 0 ? 0 : _SHOOTING_AXE_CORRECTION;

        foreach (float point in _THROW_AXES_POINT_X)
            ShootingProjectile(new Vector2((point - correction) * dir, _THROW_AXES_HIGHT_MAX), _axes);
    }

    IEnumerator EyeLaserAttack()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_EYE_LASER_ATTACK_SKILL_NUMBER];
        Animator.Play("EyeLaserAttack");

        for(int i = 0; i < skillData.NumberOfShots; i++)
        {
            ViewTarget();
            ShootingProjectile(this.MoveDir, _lasers);
            yield return new WaitForSeconds(skillData.DelayBetweenShots);
        }

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void ShootingProjectile(Vector2 dir, Queue<MonsterProjectile> projectileQueue)
    {
        MonsterProjectile shootingProjectile = projectileQueue.Dequeue();

        shootingProjectile.Object.SetActive(true);
        shootingProjectile.Projectile.ShootingProjectile(this.transform.position, dir);

        projectileQueue.Enqueue(shootingProjectile);
    }

    public void GoblinTornadoActiveHitBox()
    {
        ActiveHitBox(_HITBOX_NUM_TORNADO);
    }

}
