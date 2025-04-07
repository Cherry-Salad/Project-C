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
    public int DataId { get; private set; }
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
        DataId = Data.DataId;
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

    /// <summary>
    /// 스킬 사용 가능한지 확인한다.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSkillUsable()
    {
        // 이미 해당 스킬 애니메이션이 재생되어 있다면, 애니메이션 이벤트 호출이 꼬일 수 있다. 중복을 방지한다.
        AnimatorStateInfo stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(AnimationName))
            return false;

        // 쿨타임 완료 여부
        if (_completeCooldown == false)
            return false;

        return true;
    }

    public virtual bool DoSkill()
    {
        if (IsSkillUsable() == false) 
            return false;

        //Debug.Log($"DoSkill: {Name}");
        return true;
    }

    protected IEnumerator CoUpdateSkill(Action callback = null)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            
            if (Owner.State != ECreatureState.Skill)
                break;

            UpdateSkill();
        }

        callback?.Invoke();
    }

    public virtual void UpdateSkill()
    {
        if (Owner.CheckGround() == false)
        {
            // 공중(점프, 낙하)이라면 이동 방향에 장애물이 있을 때 제자리에서 걷는 버그 방지
            float distance = Owner.Collider.bounds.extents.x + 0.1f;
            bool noObstacles = Owner.FindObstacle(Owner.MoveDir, distance, true).collider == null; // 장애물이 없는 지 확인
            float velocityX = (noObstacles) ? Owner.MoveDir.x * Owner.MoveSpeed : 0f;   // 장애물이 있다면 수평 속도(velocity.x)를 0으로 설정

            // 점프, 낙하
            Owner.Rigidbody.velocity = new Vector2(velocityX, Owner.Rigidbody.velocity.y);
        }
        else
        {
            // 스킬 사용 중일 때 바닥에 있다면 움직이지 않는다
            Owner.Rigidbody.velocity = Vector2.zero;
        }
    }

    public virtual void EndSkill()
    {
        //Debug.Log("EndSkill");
        // 캐릭터가 공중에 있으면 점프로 전환
        Owner.State = Owner.CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    /// <summary>
    /// 히트 박스를 생성한다.
    /// </summary>
    /// <param name="canRecycle">생성된 히트 박스를 재활용할 것인가?</param>
    /// <param name="parent"></param>
    public virtual void SpawnHitBox(Vector3 spawnPos, bool canRecycle = false, Transform parent = null)
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

        HitBox.SetInfo(spawnPos, Owner, this, Owner.LookLeft, excludeLayers);
    }

    public virtual void DespawnHitBox(bool canRecycle = false)
    {
        if (HitBox == null)
            return;

        if (canRecycle)
            HitBox.gameObject.SetActive(false);
        else
            Managers.Resource.Destroy(HitBox.gameObject);
    }

    public virtual void SpawnProjectile(Vector3 spawnPos, Vector3 dir)
    {
        if (Managers.Data.ProjectileDataDic.TryGetValue(ProjectileId, out var data) == false)
            return;

        // 투사체 소환
        Projectile projectile = Managers.Resource.Instantiate(PrefabName).GetComponent<Projectile>();
        if (projectile == null) 
            return;

        // 투사체 소환 위치 설정
        projectile.transform.position = spawnPos;

        // 충돌을 제외시킬 레이어
        LayerMask excludeLayers = 0;
        excludeLayers.AddLayer(ELayer.Default);
        excludeLayers.AddLayer(ELayer.Projectile);
        excludeLayers.AddLayer(ELayer.HitBox);

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

    /// <summary>
    /// 캐스팅을 시작한다. 피격 받으면 캐스팅을 취소한다.
    /// </summary>
    /// <param name="action">캐스팅을 완료하면 호출할 이벤트</param>
    /// <returns></returns>
    protected IEnumerator CoDoCastingSkill(Action callback)
    {
        float elapsedTime = 0f;
        while (elapsedTime < CastingTime)
        {
            if (Owner.State == ECreatureState.Hurt)
            {
                yield break;
            }
            elapsedTime += Time.deltaTime;
            //Debug.Log($"elapsedTime: {elapsedTime}, CastingTime: {CastingTime}");
            yield return null;
        }
        Debug.Log($"{callback}호출 직전");
        callback?.Invoke();
    }

    protected IEnumerator CoSkillCooldown()
    {
        _completeCooldown = false;
        yield return new WaitForSeconds(CoolTime);
        _completeCooldown = true;
    }
}