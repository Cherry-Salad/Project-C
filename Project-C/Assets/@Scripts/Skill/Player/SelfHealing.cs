using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SelfHealing : PlayerSkillBase
{
    Coroutine _coCasting = null;

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

    void Update()
    {
        // 캐스팅 시작 신호가 들어올 때까지 기다린다
        if (_coCasting == null)
            return;

        if (Owner.ObjectType == EObjectType.Player)
        {
            if (Input.GetKeyUp(Key))
            {
                // 캐스팅 취소
                OnCancelCasting();
                return;
            }
        }
    }

    public override bool IsSkillUsable()
    {
        if (base.IsSkillUsable() == false)
            return false;

        // 공중에서 사용 불가능
        if (Owner.CheckGround() == false)
            return false;

        return true;
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false) 
            return false;

        Owner.State = ECreatureState.Skill;
        Owner.Animator.Play(AnimationName);
        _coCasting = StartCoroutine(CoDoCastingSkill(OnHeal));
        Debug.Log("힐 정상 작동 중...");
        
        return true;
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출한다.
    /// </summary>
    public void OnCancelCasting()
    {
        // 캐스팅을 멈춘다
        if (_coCasting != null)
        {
            StopCoroutine(_coCasting);
            _coCasting = null;
        }

        EndSkill();
    }

    void OnHeal()
    {
        Debug.Log($"Heal이 시작될 예정");
        _coCasting = null;
        Owner.Animator.Play($"{AnimationName}Complete");
        Debug.Log($"Heal이 될 예정");

        // 최대 체력을 초과해서 힐링할 수 없다
        Owner.Hp = Mathf.Clamp(Owner.Hp + HealingValue, 0, Owner.MaxHp);
        (Owner as Player)?.TriggerOnHpChanged();
        Debug.Log($"Heal {Owner.Hp}");
    }
}
