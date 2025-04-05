using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Define;
using static MonsterBase;

public class BossMonsterBase : MonsterBase
{
    private const int _GROGGY_GAUGE_ADD_VALUE = 1;
    private const float _HURT_TIME = 0.1f;

    private Coroutine vibrationCoroutine;
    private Coroutine groggyGaugeDownCoroutine;

    protected int phase;
    protected bool isInvincibility = false;
    protected int groggyGauge;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        isInvincibility = false;
        groggyGauge = 0;
        phase = 0;

        return true;
    }

    protected override void UpdateController()
    {
        if (!isCompleteLoad) return;

        if (State == ECreatureState.Dead || State == ECreatureState.Hurt) return;

        DetectingPatternChangeHp();

        if (BehaviorPattern == EBehaviorPattern.Battle || DataRecorder.TypeLevel != "Boss")
            base.UpdateController();
        
        else if(State == ECreatureState.Idle)
            SimpleStopHorizontalMove();
        
    }

    protected override void RegistrationSkill()
    {
        skillList.Clear();

        switch(phase)
        {
            case 0:
                Phase0RegistrationSkill();
                break;
            case 1:
                Phase1RegistrationSkill();
                break;
            case 2:
                Phase2RegistrationSkill();
                break;
        }
        

        shufflingSkill(skillList);
    }

    public virtual void Phase0RegistrationSkill(){

    }

    public virtual void Phase1RegistrationSkill()
    {

    }

    public virtual void Phase2RegistrationSkill()
    {

    }

    protected override void SettingSubData()
    {
    }

    public void AwakeMonster()
    {
        base.HitEnd();
        StartCoroutine(ref groggyGaugeDownCoroutine, GroggyGaugeDownUpdateCoroutine());
    }

    protected void DetectingPatternChangeHp()
    {
        if (phase >= DataRecorder.Boss.MaxPhase) return;
        
        if(hp <= DataRecorder.Boss.PhaseChangeHP[phase] * DataRecorder.MaxHP)
            ChangePhase();
    }

    public virtual bool ChangePhase()
    {
        if (phase >= DataRecorder.Boss.MaxPhase) return false;
        phase++;

        return true;
    }

    public override void Hit(int DMG = 1) // 공격 받았을 경우의 처리 
    {
        if (isInvincibility) return;
        if (State == ECreatureState.Dead) return;

        hp -= DMG;

        if (hp <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(Dead());
        }
        else
        {
            UpdateGroggyGauge(_GROGGY_GAUGE_ADD_VALUE);
            StartCoroutine(HurtCoroutine());
        }
    }

    public float HpPercent()
    {
        return (float)hp / DataRecorder.MaxHP;
    }

    protected IEnumerator HurtCoroutine()  // 데미지를 받았을 때 
    {
        try
        {
            this.SpriteRenderer.color = Color.red;
            yield return new WaitForSeconds(_HURT_TIME);
        }
        finally
        {
            this.SpriteRenderer.color = originColor;
        }
    }

    protected void UpdateGroggyGauge(int sumGroggyData)
    {
        groggyGauge += sumGroggyData;
        
        if(State != ECreatureState.Hurt && groggyGauge >= DataRecorder.Groggy.MAXGauge)
        {
            EndSkill(ECreatureState.Hurt);
            StartCoroutine(OnGroggyCoroutine());
        }
    }

    protected IEnumerator GroggyGaugeDownUpdateCoroutine()
    {
        while(true)
        {
            if (0 < groggyGauge && groggyGauge < DataRecorder.Groggy.MAXGauge)
                UpdateGroggyGauge(DataRecorder.Groggy.Decrease * -1);

            yield return new WaitForSeconds(DataRecorder.Groggy.DecreaseCycleSec);
        }
    }

    protected IEnumerator OnGroggyCoroutine()
    {
        StartCoroutine(ref vibrationCoroutine, ObjectVibrationCoroutine(30f, 0, 0.1f, 0.025f));
        AudioManager.Instance.PlaySFX(AudioManager.Instance.MonsterElite_Groggy); //Groggy SFX 재생
        isCanAttack = false;

        yield return new WaitForSeconds(DataRecorder.Groggy.DurationTime);

        State = ECreatureState.Idle;
        isCanAttack = true;
        groggyGauge = 0;

        StopCoroutine(ref vibrationCoroutine);
    }

    protected override bool SearchingTargetInBattleState() 
    {
        return true;
    }
}
