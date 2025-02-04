using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GoblinBoomerangThrower : MonsterBase
{
    private const int _HITBOX_NUM_BODY = 0;

    private const int _THROW_BOOMERANG_SKILL_NUMBER = 0;

    private Queue<MonsterProjectile> _boomerangs = new Queue<MonsterProjectile>();
    
    private int MAX_BOOMERANG_COUNT = 3;

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

    protected override void SettingProjectile()
    {
        for (int i = 0; i < MAX_BOOMERANG_COUNT; i++)
            _boomerangs.Enqueue(MakeProjectile(_THROW_BOOMERANG_SKILL_NUMBER));
        
    }

    IEnumerator ThrowBoomerang()
    {
        Data.MonsterSkillData skillData = TypeRecorder.Battle.Attack[_THROW_BOOMERANG_SKILL_NUMBER];

        yield return new WaitForSeconds(skillData.RecoveryTime);
    }

    public void ShootingProjectile()
    {
        MonsterProjectile throwBoomerang = _boomerangs.Dequeue();

        throwBoomerang.Object.SetActive(true);
        throwBoomerang.Projectile.ShootingProjectile(TargetGameObject);
       
        _boomerangs.Enqueue(throwBoomerang);
    }
}
