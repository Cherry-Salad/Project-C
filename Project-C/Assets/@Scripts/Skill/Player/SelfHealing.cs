using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SelfHealing : PlayerSkillBase
{
    public float PressedTime = 0.3f;

    bool _isCasting = false;
    Coroutine _coCasting = null;
    float _castingStartTime = 0;

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
        if (_isCasting == false)
            return;

        if (Owner.ObjectType == EObjectType.Player)
            GetInput();
    }

    void GetInput()
    {
        if (Input.GetKeyUp(Key))
        {
            // 캐스팅 취소
            OnCancelCasting();
            return;
        }

        // 캐스팅을 이미 시작하고 있다면 
        if (_coCasting != null)
            return;

        float pressedTime = Time.time - _castingStartTime;
        if (pressedTime > PressedTime)
        {
            // 꾹 눌러야 캐스팅 시작
            Owner.Animator.Play(AnimationName);
            _coCasting = StartCoroutine(CoDoCastingSkill(OnHeal));
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

        // 캐스팅 시작
        _isCasting = true;
        _castingStartTime = Time.time;
        
        return true;
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출한다.
    /// </summary>
    public void OnCancelCasting()
    {
        Debug.Log("EndSkill");

        _isCasting = false;
        _castingStartTime = 0f;

        // 캐스팅을 멈춘다
        if (_coCasting != null)
        {
            StopCoroutine(_coCasting);
            _coCasting = null;
        }

        // 캐릭터가 공중에 있으면 점프로 전환
        Owner.State = Owner.CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    void OnHeal()
    {
        Owner.Animator.Play($"{AnimationName}Complete");

        // 최대 체력을 초과해서 힐링할 수 없다
        Owner.Hp = Mathf.Clamp(Owner.Hp + HealingValue, Owner.Hp + HealingValue, Owner.MaxHp);
        Debug.Log($"Heal {Owner.Hp}");

        // 마나 소비
        //Owner.Mp -= MpCost;   // 테스트를 위하여 마나 소비는 껐다
    }
}
