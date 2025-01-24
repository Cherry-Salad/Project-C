using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Projectile : BaseObject
{
    public Creature Owner { get; private set; }
    public SkillBase Skill { get; private set; }
    public Data.ProjectileData ProjectileData { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Projectile;
        return true;
    }

    /// <summary>
    /// 투사체 정보를 셋팅한다.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="skill"></param>
    /// <param name="data"></param>
    /// <param name="excludeLayers">충돌을 제외할 레이어</param>
    public void SetInfo(Creature owner, SkillBase skill, ProjectileData data, LayerMask excludeLayers)
    {
        Owner = owner;
        Skill = skill;
        ProjectileData = data;

        // 중력
        DefaultGravityScale = data.DefaultGravity;

        // 충돌을 제외할 레이어 필터링
        Collider.excludeLayers = excludeLayers;

        // TODO: 방향대로 투사체가 발사된다

        // 투사체 유지 시간이 지나면 사라진다
        StartCoroutine(CoDestroy(data.LifeTime));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D");

        BaseObject target = collision.GetComponent<BaseObject>();
        if (target != null)
        {
            target.OnDamaged(Skill.DamageMultiplier, Owner);
        }

        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoDestroy(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Resource.Destroy(gameObject);
    }
}
