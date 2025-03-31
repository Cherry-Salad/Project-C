using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class GoblinBoomerangThrower : MonsterBase
{
    private const int _HITBOX_NUM_BODY = 0;

    private const int _THROW_BOOMERANG_SKILL_NUMBER = 0;

    private Queue<MonsterProjectile> _boomerangs = new Queue<MonsterProjectile>();
    private MonsterProjectile throwBoomerang;

    private bool _isThrow = false;
    private bool _isReady = true;
    private int MAX_BOOMERANG_COUNT = 3;

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
        skillList.Add(new Tuple<int, IEnumerator>(_THROW_BOOMERANG_SKILL_NUMBER, ThrowBoomerang()));

        shufflingSkill(skillList);
    }

    protected override void SettingProjectile()
    {
        for (int i = 0; i < MAX_BOOMERANG_COUNT; i++)
            _boomerangs.Enqueue(MakeProjectile(_THROW_BOOMERANG_SKILL_NUMBER));
    }

    protected override void HitEnd()
    {
        base.HitEnd();
        _isThrow = false;
    }

    IEnumerator ThrowBoomerang()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_THROW_BOOMERANG_SKILL_NUMBER];

        _isReady = true;
        UpdateAnimation();
        AudioManager.Instance.PlaySFXAfterDelay(AudioManager.Instance.MonsterBomerang_Shot, 0.4f); //고블린 부메랑 Shot SFX 재생
        AudioManager.Instance.PlaySFXAfterDelay(AudioManager.Instance.MonsterBomerang_Flying, 0.5f); //고블린 부메랑 Flying SFX 재생

        yield return new WaitForSeconds(skillData.WindUpTime);

        if (State != ECreatureState.Skill) yield break;

        _isReady = false;
        UpdateAnimation();

        ShootingProjectile();
        
        while(throwBoomerang.Object.activeSelf)
        {
            yield return new WaitForEndOfFrame();
        }
        
        _isThrow = false;

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    private Vector2 selectDirection()
    {
        return new Vector2(TargetGameObject.transform.position.x - this.transform.position.x, TargetGameObject.transform.position.y - this.transform.position.y);
    }

    public void ShootingProjectile()
    {
        throwBoomerang = _boomerangs.Dequeue();
        
        throwBoomerang.Object.SetActive(true);
        throwBoomerang.Projectile.ShootingProjectile(this.transform.position, selectDirection());
       
        _boomerangs.Enqueue(throwBoomerang);
    }

    protected void OnTriggerExit2D(Collider2D collider)
    {
        
        if (throwBoomerang != null && collider.gameObject == throwBoomerang.Object)
        {
            _isThrow = true;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isThrow && collider.gameObject == throwBoomerang.Object)
        {
            throwBoomerang.Projectile.EndOfProjectile();
            _isThrow = false;
        }
    }

    protected override void DeadOrganize()
    {
        foreach (MonsterProjectile boomerang in _boomerangs)
        {
            GameObject.Destroy(boomerang.Object);
        }
    }
}
