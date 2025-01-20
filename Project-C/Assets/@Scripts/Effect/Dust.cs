using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Dust : InitBase
{
    public Creature Owner { get; set; }

    public SpriteRenderer SpriteRenderer { get; protected set; }
    public Animator Animator { get; protected set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();

        return true;
    }

    public void PlayEffect(Creature owner)
    {
        Owner = owner;

        switch (owner.State)
        {
            case ECreatureState.Dash:
                PlayDashEffect("Dash");
                break;

            case ECreatureState.WallCling:
                StartCoroutine(CoPlayWallClingEffect("WallSlide", Vector2.zero));
                break;

            case ECreatureState.Dead:
                // TODO
                break;
        }
    }

    void PlayDashEffect(string name)
    {
        float offset = 1.2f;    // 먼지가 플레이어에서 얼마나 떨어질지 조정
        if (Owner.LookLeft)
            // 플레이어 왼쪽
            transform.position = new Vector2(Owner.Collider.bounds.min.x + offset, Owner.Collider.bounds.max.y);
        else
            // 플레이어 오른쪽
            transform.position = new Vector2(Owner.Collider.bounds.max.x - offset, Owner.Collider.bounds.max.y);

        SpriteRenderer.flipX = Owner.LookLeft;
        Animator.Play(name);
    }

    IEnumerator CoPlayWallClingEffect(string name, Vector2 position)
    {
        transform.parent = Owner.transform;
        transform.localPosition = position;

        SpriteRenderer.flipX = Owner.LookLeft;
        Animator.Play(name);

        while (Owner.State == ECreatureState.WallCling)
        {
            // WallCling 먼지 효과 애니메이션 재생 중
            yield return null;
        }

        transform.parent = null;

        // WallCling이 아니더라도, 남은 애니메이션 시간이 있다면 끝까지 재생한 뒤에 사라진다
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            // 애니메이션 남은 시간 계산
            float clipLength = stateInfo.length;
            float elapsedTime = clipLength * stateInfo.normalizedTime;
            float remainingTime = clipLength - elapsedTime - 0.1f;

            // 남은 시간만큼 재생
            if (remainingTime > 0)
                yield return new WaitForSeconds(remainingTime);
        }

        Despawn();
    }

    public void Despawn()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
