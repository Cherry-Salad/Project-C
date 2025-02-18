using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class IceBall : PlayerSkillBase
{
    Vector2 _skillDir { get { return (Owner.LookLeft) ? Vector2.left : Vector2.right; } }

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
        return base.IsSkillUsable();
    }

    public override bool DoSkill()
    {
        if (base.DoSkill() == false) 
            return false;

        Debug.Log("DoSkill");
        
        Owner.State = ECreatureState.Skill;
        Owner.Animator.Play(AnimationName);
        StartCoroutine(CoUpdateSkill(() => Owner.Rigidbody.gravityScale = Owner.DefaultGravityScale));
        //Owner.Mp -= MpCost;   // 테스트를 위하여 마나 소비는 껐다
        return true;
    }

    public override void UpdateSkill()
    {
        Owner.Rigidbody.gravityScale = 0f;
        Owner.Rigidbody.velocity = Vector2.zero;
    }

    public override void EndSkill()
    {
        base.EndSkill();
        Owner.Rigidbody.gravityScale = Owner.DefaultGravityScale;
    }

    void OnSpawnIceBall()
    {
        Vector3 offset = (Owner.LookLeft) ? new Vector3(-0.5f, -0.05f, 0) : new Vector3(0.5f, -0.05f, 0);
        SpawnProjectile(Owner.transform.position + offset, (Vector3)_skillDir);
    }

    void OnEndSkill()
    {
        EndSkill();
    }
}
