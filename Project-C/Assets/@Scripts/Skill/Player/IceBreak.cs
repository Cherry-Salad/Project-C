using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class IceBreak : PlayerSkillBase
{
    Vector2 _skillDir { get { return (Owner.LookLeft) ? Vector2.left : Vector2.right; } }
    HitBox _hitBox;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(Creature owner, SkillData data)
    {
        base.SetInfo(owner, data);
    }

    public override bool IsSkillUsable()
    {
        if (base.IsSkillUsable() == false)
            return false;

        if (Owner.CheckGround() == false)
            return false;

        return true;
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false)
            return false;

        //Owner.Mp -= MpCost;

        Owner.Animator.Play(AnimationName);
        Owner.State = ECreatureState.Skill;
        StartCoroutine(CoDoSkill());

        return true;
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출하며, 히트 박스를 생성한다.
    /// </summary>
    void OnSpawnIceBreak()
    {
        GameObject go = Managers.Resource.Instantiate(PrefabName, transform);
        _hitBox = go.GetComponent<HitBox>();
        
        // 충돌을 제외할 레이어 필터링
        LayerMask excludeLayers = 0;
        excludeLayers.AddLayer(ELayer.Default);
        excludeLayers.AddLayer(ELayer.Ground);
        excludeLayers.AddLayer(ELayer.Wall);

        switch (Owner.ObjectType)
        {
            case EObjectType.Player:
                excludeLayers.AddLayer(ELayer.Player);
                break;
            case EObjectType.Monster:
                excludeLayers.AddLayer(ELayer.Monster);
                break;
        }

        _hitBox.SetInfo(Owner.LookLeft, DamageMultiplier, excludeLayers);
        _hitBox.transform.parent = null;
    }

    IEnumerator CoDoSkill()
    {
        AnimatorStateInfo stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(AnimationName) == false)
        {
            // 스킬 애니메이션 재생 대기 중
            yield return null;
            stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
        }

        float duration = stateInfo.length + RecoveryTime;   // (애니메이션 길이) + (후 딜레이)
        float elapsedTime = 0f;

        // 스킬 애니메이션 재생 중
        while (elapsedTime < duration)
        {
            // 피격 시 스킬 취소
            if (Owner.State == ECreatureState.Hurt || stateInfo.IsName(AnimationName) == false)
            {
                Managers.Resource.Destroy(_hitBox.gameObject);
                yield break;
            }

            stateInfo = Owner.Animator.GetCurrentAnimatorStateInfo(0);
            elapsedTime = duration * stateInfo.normalizedTime;
            //Debug.Log($"duration: {duration:F2}, elapsedTime: {elapsedTime:F2}");
            yield return null;
        }

        EndSkill();
    }
}
