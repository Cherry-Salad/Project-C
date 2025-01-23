using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SelfHealing : PlayerSkillBase
{
    bool _isCasting = false;
    Coroutine _casting = null;
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
        if (_isCasting == false)
            return;

        if (Owner.ObjectType == EObjectType.Player)
            GetInput();
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
        
        _isCasting = true;  // 캐스팅 시작
        _castingStartTime = Time.time;
        return true;
    }

    void GetInput()
    {
        if (Input.GetKeyUp(Key))
        {
            // 캐스팅 취소
            CancelCasting();
            return;
        }

        float pressedTime = Time.time - _castingStartTime;
        if (_casting == null && pressedTime > 0.5f)
        {
            // 꾹 눌러야 캐스팅 시작
            Owner.Animator.Play(AnimationName);
            _casting = StartCoroutine(CoDoCastingSkill(Heal));
        }
    }

    /// <summary>
    /// 애니메이션 이벤트로 호출한다.
    /// </summary>
    public void CancelCasting()
    {
        Debug.Log("EndSkill");

        _isCasting = false;
        _castingStartTime = 0f;

        if (_casting != null)
        {
            StopCoroutine(_casting);
            _casting = null;
        }

        // 캐릭터가 공중에 있으면 점프로 전환
        Owner.State = Owner.CheckGround() ? ECreatureState.Idle : ECreatureState.Jump;
    }

    void Heal()
    {
        Owner.Animator.Play($"{AnimationName}Complete");

        // 최대 체력을 초과해서 힐링할 수 없다
        Owner.Hp = Mathf.Clamp(Owner.Hp + HealingValue, Owner.Hp + HealingValue, Owner.MaxHp);
        Debug.Log($"Heal {Owner.Hp}");

        // 마나 소비
        Owner.Mp -= MpCost;
    }
}
