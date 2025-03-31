using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class GoblinElite : BossMonsterBase
{
    [SerializeReference] private GameObject _fMark;

    private const int _HITBOX_NUM_BODY = 0;
    private const int _HITBOX_NUM_TORNADO = 1;
    private const int _HITBOX_NUM_PUNCH = 2;

    private const int _EFFECT_NUM_PUNCH = 0;

    private const int _TORNADO_SKILL_NUMBER = 0;
    private const int _THROWING_AXE_SKILL_NUMBER = 1;
    private const int _EYE_LASER_ATTACK_SKILL_NUMBER = 2;
    private const int _EXPLOSION_PUNCH_SKILL_NUMBER = 3;

    private const int _MAX_AXE_COUNT = 20;
    private const int _MAX_LASER_COUNT = 20;

    private const float _JUMP_POWER = 10;

    private float[] _THROW_AXES_POINT_X = {0.5f, 1f, 1.5f, 2.0f, 2.5f};
    private const float _THROW_AXES_HIGHT_MAX = 2f;
    private const float _SHOOTING_AXE_CORRECTION = 0.25f;
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
        DeactivateEffect();

        _fMark.SetActive(false);

        return true;
    }

    protected override void SettingProjectile()
    {
        for (int i = 0; i < _MAX_AXE_COUNT; i++)
            _axes.Enqueue(MakeProjectile(_THROWING_AXE_SKILL_NUMBER));
        
        for (int i = 0; i < _MAX_LASER_COUNT; i++)
            _lasers.Enqueue(MakeProjectile(_EYE_LASER_ATTACK_SKILL_NUMBER));
    }

    public override void Phase0RegistrationSkill()
    {
        skillList.Add(new Tuple<int, IEnumerator>(_THROWING_AXE_SKILL_NUMBER, ThrowingAxe()));
        skillList.Add(new Tuple<int, IEnumerator>(_EYE_LASER_ATTACK_SKILL_NUMBER, EyeLaserAttack()));
        skillList.Add(new Tuple<int, IEnumerator>(_EXPLOSION_PUNCH_SKILL_NUMBER, ExplosionPunch()));
    }

    public override void Phase1RegistrationSkill()
    {
        skillList.Add(new Tuple<int, IEnumerator>(_TORNADO_SKILL_NUMBER, GoblinTornado()));
        skillList.Add(new Tuple<int, IEnumerator>(_THROWING_AXE_SKILL_NUMBER, ThrowingAxe()));
        skillList.Add(new Tuple<int, IEnumerator>(_EYE_LASER_ATTACK_SKILL_NUMBER, EyeLaserAttack()));
        skillList.Add(new Tuple<int, IEnumerator>(_EXPLOSION_PUNCH_SKILL_NUMBER, ExplosionPunch()));
    }

    IEnumerator GoblinTornado()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_TORNADO_SKILL_NUMBER];
        PopupEMark();
        yield return new WaitForSeconds(skillData.WindUpTime);

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

        Vector2 jumpVector = new Vector2(this.MoveDir.x * -1, 1).normalized;

        Rigidbody.velocity = jumpVector * _JUMP_POWER;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_Axe_BackStep); //고블린 엘리트 axe Throw 재생
        yield return new WaitForSeconds(skillData.WindUpTime);
        SimpleStopHorizontalMove();

        while(!CheckGround())
            yield return null;
        SimpleStopMove();

        for (_shootingAxeCount = 0; _shootingAxeCount < skillData.NumberOfShots; _shootingAxeCount++)
        {
            if (TargetGameObject == null) break;
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
        {
            ShootingProjectile(new Vector2((point - correction) * dir, _THROW_AXES_HIGHT_MAX), _axes);
        }
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_Axe_Throw); //고블린 엘리트 axe Throw 재생
    }

    IEnumerator EyeLaserAttack()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_EYE_LASER_ATTACK_SKILL_NUMBER];
        Animator.Play("EyeLaserAttack");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_Laser_MaskUP); //고블린 엘리트 Laser MaskUp 재생
        yield return new WaitForSeconds(skillData.WindUpTime);

        for (int i = 0; i < skillData.NumberOfShots; i++)
        {
            if (TargetGameObject == null) break;
            ViewTarget();
            ShootingProjectile(this.MoveDir, _lasers);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_Laser_Shot); //고블린 엘리트 Laser Shot재생
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

    IEnumerator ExplosionPunch()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_EXPLOSION_PUNCH_SKILL_NUMBER];

        for(int i = 0; i < skillData.NumberOfShots; i++)
        {
            ActiveEffect(_EFFECT_NUM_PUNCH);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_ExplosionPunch_Charge); //고블린 엘리트 ExplosionPunch Charge 재생
            yield return new WaitUntil(() => !effectList[_EFFECT_NUM_PUNCH].activeSelf);
            yield return new WaitForSeconds(skillData.DelayBetweenShots);
        }

        Animator.Play("ExplosionPunch");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_ExplosionPunch_Punch); //고블린 엘리트 ExplosionPunch Punch 재생
        AudioManager.Instance.PlaySFXAfterDelay(AudioManager.Instance.MonsterElite_ExplosionPunch_Explosion, 0.5f); //0.5f초 뒤에 고블린 엘리트 ExplosionPunch Explosion 재생

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void GoblinExplosionPunchActiveHitBox()
    {
        ActiveHitBox(_HITBOX_NUM_PUNCH);
    }

    protected override IEnumerator Dead() //죽었을 때 처리 
    {
        State = ECreatureState.Dead;
        Color color = this.SpriteRenderer.color;

        try
        {
            Animator.Play("die");
            yield return new WaitForSeconds(1f);

            for (float i = 1f; i >= 0; i -= 0.1f)
            {
                color.a = i;
                SpriteRenderer.color = color;
                DeadOrganize();
                yield return new WaitForSeconds(0.1f);
            }
        }
        finally
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public override bool ChangePhase()
    {
        if(base.ChangePhase()) 
        {
            StartCoroutine(popUpStateTransitionIconCoroutine(_fMark));
            skillList.Add(new Tuple<int, IEnumerator>(_TORNADO_SKILL_NUMBER, GoblinTornado()));
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_Phase2); //고블린 엘리트 Phase2 재생
        return base.ChangePhase();
    }

    protected override void DeadOrganize()
    {
        foreach (MonsterProjectile axe in _axes)
            GameObject.Destroy(axe.Object);
        
        foreach (MonsterProjectile laser in _lasers)
            GameObject.Destroy(laser.Object);
    }
}
