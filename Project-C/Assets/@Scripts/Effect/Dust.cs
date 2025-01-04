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
                PlayDash("Dash");
                break;

            case ECreatureState.WallCling:
                StartCoroutine(CoPlayWallCling("WallSlide", Vector2.zero));
                break;

            case ECreatureState.Dead:
                // TODO
                break;
        }
    }

    void PlayDash(string name)
    {
        float offset = 1.2f;    // 먼지가 플레이어에서 얼마나 떨어질지 조정
        if (Owner.LookLeft)
        {
            // 플레이어 왼쪽
            transform.position = new Vector2(Owner.Collider.bounds.min.x + offset, Owner.Collider.bounds.max.y);
        }
        else
        {
            // 플레이어 오른쪽
            transform.position = new Vector2(Owner.Collider.bounds.max.x - offset, Owner.Collider.bounds.max.y);
        }

        SpriteRenderer.flipX = Owner.LookLeft;
        Animator.Play(name);
    }

    IEnumerator CoPlayWallCling(string name, Vector2 position)
    {
        transform.parent = Owner.transform;
        transform.localPosition = position;

        SpriteRenderer.flipX = Owner.LookLeft;
        Animator.Play(name);

        while (Owner.State == ECreatureState.WallCling)
        {
            SpriteRenderer.flipX = Owner.LookLeft;
            yield return null;
        }

        if (Owner.State == ECreatureState.WallClimbing)
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            // 애니메이션 남은 시간 계산
            float clipLength = stateInfo.length;
            float elapsedTime = clipLength * (stateInfo.normalizedTime % 1);
            float remainingTime = clipLength - elapsedTime;

            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime - 0.1f); // 남은 시간만큼 대기
                Hide();
            }
        }

        Hide();
    }

    public void Hide()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
