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

        // TODO: 지금은 일직선으로 발사 밖에 없지만, 발사 종류에 따라 달라진다.
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
        else if (Data.LifeTime > 0)
        {
            // 목적지가 없다면 lifeTime동안 투사체를 발사한다
            Rigidbody.velocity = dir * _speed;
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 투사체가 부숴지는 순간이므로, 충돌과 속력이 없다
        Rigidbody.velocity = Vector2.zero;
        Collider.isTrigger = false;

        // SetInfo에서 충돌 대상을 다 필터링해서, BaseObject만 찾으면 된다.
        // 하지만, 몬스터는 Hit 함수가 따로 있어서 일단 구분하였다.
        BaseObject target = collision.GetComponent<BaseObject>();
        if (target != null)
        {
            Debug.Log("OnTriggerEnter2D");

            float damage = Owner.Atk * Skill.DamageMultiplier;  // 오너 공격력 * 스킬 공격력
            MonsterBase monster = target as MonsterBase;

            if (monster != null)
            {
                // 버그 확인 필요: 아주 간혹, 몬스터의 BodyHitBox가 비활성되어 피격 처리가 안된다.
                // Test를 위하여 공격력을 낮췄으니 직접 테스트 해보길 바랍니다. 
                //monster.Hit((int)damage);
                monster.Hit();  // Test
            }
            else
                target.OnDamaged(damage, Owner);
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
