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

    #region Info
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
    #endregion

    public HitBox HitBox { get; protected set; }
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

    /// <summary>
    /// 히트 박스를 생성한다.
    /// </summary>
    /// <param name="spawnPos">소환 위치</param>
    /// <param name="canRecycle">생성된 히트 박스를 재활용할 것인가?</param>
    /// <param name="parent"></param>
    public virtual void SpawnHitBox(Vector3 spawnPos, bool canRecycle = false, Transform parent = null)
    {
        SpawnHitBox(canRecycle, parent);
        HitBox.transform.localPosition = spawnPos;  // 소환 위치 설정
    }

    /// <summary>
    /// 히트 박스를 생성한다.
    /// </summary>
    /// <param name="canRecycle">생성된 히트 박스를 재활용할 것인가?</param>
    /// <param name="parent"></param>
    public virtual void SpawnHitBox(bool canRecycle = false, Transform parent = null)
    {
        // 히트 박스 생성
        if (canRecycle == false || (canRecycle && HitBox == null))
        {
            GameObject go = Managers.Resource.Instantiate(PrefabName, parent);
            HitBox = go.GetComponent<HitBox>();
        }

        // 충돌을 제외시킬 레이어
        LayerMask excludeLayers = HitBox.Collider.excludeLayers;

        // excludeLayers 없다면 필터링 설정
        if (excludeLayers.value == 0)
        {
            excludeLayers.AddLayer(ELayer.Default);
            excludeLayers.AddLayer(ELayer.Ground);
            excludeLayers.AddLayer(ELayer.Wall);

            // 자기 자신은 제외
            switch (Owner.ObjectType)
            {
                case EObjectType.Player:
                    excludeLayers.AddLayer(ELayer.Player);
                    break;
                case EObjectType.Monster:
                    excludeLayers.AddLayer(ELayer.Monster);
                    break;
            }
        }

        HitBox.SetInfo(Owner, this, Owner.LookLeft, excludeLayers);
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

        // 충돌을 제외시킬 레이어
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
        //Debug.Log("EndSkill");

        // 캐릭터가 공중에 있으면 점프로 전환
        Owner.State = Owner.CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    /// <summary>
    /// 캐스팅을 시작한다. 피격 당하면 캐스팅을 취소한다.
    /// </summary>
    /// <param name="action">캐스팅을 완료하면 호출할 이벤트</param>
    /// <returns></returns>
    protected IEnumerator CoDoCastingSkill(Action callback)
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

        callback?.Invoke();
    }

    protected IEnumerator CoSkillCooldown()
    {
        _completeCooldown = false;
        yield return new WaitForSeconds(CoolTime);
        _completeCooldown = true;
    }
}