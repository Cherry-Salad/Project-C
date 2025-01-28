using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SkillBase : InitBase
{
    public Creature Owner { get; private set; }
    public SkillData Data;

    public string Name { get; protected set; }
    public string AnimationName { get; protected set; }
    public string PrefabName { get; protected set; }
    public int ProjectileId { get; protected set; }
    public float CastingTime { get; protected set; }    // 시전 시간
    public float RecoveryTime { get; protected set; }   // 후 딜레이
    public float CoolTime { get; protected set; }   // 시전 후 회복 시간
    public float HealingValue { get; protected set; }   // 회복량
    public float DamageMultiplier { get; protected set; }   // 데미지 배율
    public float AttackRange { get; protected set; }    // 공격 범위

    protected bool _completeCooldown = true;  // 쿨타임 완료 여부

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public virtual void SetInfo(Creature owner, SkillData data)
    {
        Owner = owner;
        Data = data;

        if (data == null)
            return;

        #region 스킬 정보
        Name = Data.CodeName;
        AnimationName = Data.AnimationName;
        PrefabName = Data.PrefabName;
        ProjectileId = Data.ProjectileId;
        CastingTime = Data.CastingTime;
        RecoveryTime = Data.RecoveryTime;
        CoolTime = Data.CoolTime;
        HealingValue = Data.HealingValue;
        DamageMultiplier = Data.DamageMultiplier;
        AttackRange = Data.AttackRange;
        #endregion
    }

    public virtual bool IsSkillUsable()
    {
        // 이미 스킬 애니메이션이 재생되어 있다면 이벤트 호출이 꼬일 수 있으므로, 중복 방지
        AnimatorStateInfo stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(AnimationName))
            return false;

        if (_completeCooldown == false)
            return false;

        return true;
    }

    public virtual bool DoSkill()
    {
        if (IsSkillUsable() == false) 
            return false;

        Debug.Log($"DoSkill: {Name}");
        return true;
    }

    public virtual void SpawnProjectile(Vector3 spawnPos, Vector3 dir)
    {
        if (Managers.Data.ProjectileDataDic.TryGetValue(ProjectileId, out var data) == false)
            return;

        Projectile projectile = Managers.Resource.Instantiate(PrefabName).GetComponent<Projectile>();
        if (projectile == null) 
            return;

        // 투사체 소환 위치 설정
        projectile.transform.position = spawnPos;

        // 충돌을 제외할 레이어 필터링
        LayerMask excludeLayers = 0;
        excludeLayers.AddLayer(ELayer.Default);
        excludeLayers.AddLayer(ELayer.Projectile);

        switch (Owner.ObjectType)
        {
            case EObjectType.Player:
                excludeLayers.AddLayer(ELayer.Player);
                break;
            case EObjectType.Monster:
                excludeLayers.AddLayer(ELayer.Monster);
                break;
        }

        projectile.SetInfo(Owner, this, data, excludeLayers, dir);
    }

    public virtual void EndSkill()
    {
        Debug.Log("EndSkill");

        // 캐릭터가 공중에 있으면 점프로 전환
        Owner.State = Owner.CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    /// <summary>
    /// 캐스팅을 시작한다. 피격 당하면 캐스팅을 취소한다.
    /// </summary>
    /// <param name="action">캐스팅을 완료하면 호출할 이벤트</param>
    /// <returns></returns>
    protected IEnumerator CoDoCastingSkill(Action action)
    {
        float elapsedTime = 0f;
        while (elapsedTime < CastingTime)
        {
            // 피격 받으면 캐스팅 취소
            if (Owner.State == ECreatureState.Hurt)
                yield break;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        action();
    }

    protected IEnumerator CoSkillCooldown()
    {
        _completeCooldown = false;
        yield return new WaitForSeconds(CoolTime);
        _completeCooldown = true;
    }
}