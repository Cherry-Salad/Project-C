using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Projectile : BaseObject
{
    public Creature Owner { get; private set; }
    public SkillBase Skill { get; private set; }
    public Data.ProjectileData Data { get; private set; }

    float _speed;
    float _range;

    Coroutine _coLaunch;

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
    /// <param name="dir">발사할 투사체 방향</param>
    public void SetInfo(Creature owner, SkillBase skill, ProjectileData data, LayerMask excludeLayers, Vector2 dir)
    {
        Debug.Log("Spawn Projectile");

        Owner = owner;
        Skill = skill;
        Data = data;

        _speed = data.BaseSpeed;
        _range = skill.Data.AttackRange;

        // 중력
        DefaultGravityScale = data.DefaultGravity;

        // 충돌을 제외할 레이어 필터링
        Collider.excludeLayers = excludeLayers;

        // 방향대로 투사체가 발사된다
        _coLaunch = StartCoroutine(CoLaunchStraight(dir));

        // 투사체 유지 시간이 지나면 사라진다
        StartCoroutine(CoDestroy(data.LifeTime));
    }

    public IEnumerator CoLaunchStraight(Vector3 dir)
    {
        if (_range > 0)
        {
            // 목적지가 있다면
            Vector3 destPos = transform.position + (dir * _range);

            while (dir.magnitude > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, destPos, _speed * Time.deltaTime);
                dir = destPos - transform.position;
                yield return null;
            }

            transform.position = destPos;
        }
        else
        {
            // 목적지가 없다면 lifeTime동안 투사체는 발사된다
            Rigidbody.velocity = dir * _speed;
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D");
        
        // 투사체가 부숴지는 순간이므로, 충돌과 속력이 없다
        Rigidbody.velocity = Vector2.zero;
        Collider.isTrigger = false;

        BaseObject target = collision.GetComponent<BaseObject>();
        if (target != null)
        {
            target.OnDamaged(Skill.DamageMultiplier, Owner);
        }

        StopCoroutine(_coLaunch);
        Animator.Play("Destroy");
    }

    void OnDestroy()
    {
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoDestroy(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);

        // 투사체가 부숴지는 순간이므로, 충돌과 속력이 없다
        Rigidbody.velocity = Vector2.zero;
        Collider.isTrigger = false;

        StopCoroutine(_coLaunch);
        Animator.Play("Destroy");
    }
}
